// This is the main DLL file.

#include "stdafx.h"

#include "NLBFGS.h"
#include "lbfgs.h"

using namespace System::Runtime::InteropServices;

namespace NLBFGS
{
	array<double>^ Optimizer::Optimize(int varsCount, Func<array<double>^, double, Tuple<double, array<double>^ >^ >^ evaluate, array<double>^ initial)
	{
		Optimizer^ optimizer = gcnew Optimizer(varsCount, evaluate);
		return optimizer->Optimize(initial);
	}

	array<double>^ Optimizer::Optimize(array<double>^ initial)
	{
		OptimizerWrapper wrapper;
		wrapper.optimizer = gcroot<Optimizer^>(this);

		double* x = new double[varsCount];
		Marshal::Copy(initial, 0, IntPtr(x), varsCount);

		int status = lbfgs(
			varsCount,  // problem dimension
			x,          // initial guess for the minimizer / output of the minimization
			NULL,       // out parameter for function value. We ignore the function value.
			&(OptimizerWrapper::Evaluate),  // evaluation function pointer
			NULL,       // progress function pointer. We don't report progress
			&wrapper,   // instance passed to the evaluation function. Needed to pass the Wrapper to Evaluate.
			NULL);      // minimization parameters. We use default parameters (NULL)

		array<double>^ result = gcnew array<double>(varsCount);
		Marshal::Copy(IntPtr(x), result, 0, varsCount);

		return result;
	}

	double OptimizerWrapper::Evaluate(void* instance, const double* x, double* g, const int n, const double step)
	{
		OptimizerWrapper* wrapper = (OptimizerWrapper*)instance;
		return wrapper->optimizer->Evaluate(IntPtr(const_cast<double*>(x)), IntPtr(g), n, step);
	}

	double Optimizer::Evaluate(IntPtr x, IntPtr g, int n, double step)
	{
		array<double>^ xArray = gcnew array<double>(n);
		Marshal::Copy(x, xArray, 0, n);

		Tuple<double, array<double>^ >^ result = evaluate->Invoke(xArray, step);
		Marshal::Copy(result->Item2, 0, g, n);

		return result->Item1;
	}

}
