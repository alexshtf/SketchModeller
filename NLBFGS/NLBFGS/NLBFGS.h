// NLBFGS.h

#pragma once
#include <vcclr.h>

using namespace System;

namespace NLBFGS {


	public ref class Optimizer
	{
	public:
		static array<double>^ Optimize(int varsCount, Func<array<double>^, double, Tuple<double, array<double>^ >^ >^ evaluate, array<double>^ initial);
	internal:	
		double Evaluate(IntPtr x, IntPtr g, int n, double step);
	private:
		Func<array<double>^, double, Tuple<double, array<double>^ >^ >^ evaluate;
		int varsCount;
		
		Optimizer(int initVarsCount, Func<array<double>^, double, Tuple<double, array<double>^ >^ >^ initEvaluate)
			: evaluate(initEvaluate)
			, varsCount(initVarsCount)
		{
		}

		array<double>^ Optimize(array<double>^ initial);

	};

	private class OptimizerWrapper
	{
	public:
		gcroot<Optimizer^> optimizer;
		static double Evaluate(void* instance, const double* x, double* g, const int n, const double step);
	};
}
