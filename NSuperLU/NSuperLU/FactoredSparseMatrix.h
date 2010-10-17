#pragma once

using namespace System;

extern "C"
{
#include "slu_ddefs.h"
}

namespace NSuperLU
{
	public ref class FactoredSparseMatrix
	{
	public:
		FactoredSparseMatrix(array<Tuple<int, int, double>^ >^ items, int dimension);
		~FactoredSparseMatrix();
		array<double>^ Solve(array<double>^ rhs);
	private:
		SuperMatrix* L;
		SuperMatrix* U;
		int* permr;
		int* permc;
	};
}