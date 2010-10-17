#include "stdafx.h"


#include "FactoredSparseMatrix.h"


NSuperLU::FactoredSparseMatrix::FactoredSparseMatrix(array<Tuple<int, int, double>^ >^ items, int dimension)
{
	// we create a compressed-row matrix
	int nnz = items->Length;
	double* values = new double[nnz];
	int* colInd = new int[nnz];
	int* rowPtr = new int[dimension + 1];

	int lastRow = -1;
	for(int i = 0; i < nnz; ++i)
	{
		int row = items[i]->Item1;
		int col = items[i]->Item2;
		double val = items[i]->Item3;

		values[i] = val;
		colInd[i] = col;

		if (lastRow < row)
		{
			for(int j = lastRow + 1; j <= row; ++j)
				rowPtr[j] = i;
		}
		lastRow = row;
	}

	// fill the rest of the values in rowPtr to be nnz
	for(int i = lastRow + 1; i <= dimension; ++i)
		rowPtr[i] = nnz;

	SuperMatrix A;
	dCreate_CompRow_Matrix(
		&A,								// the matrix to allocate
		dimension,						// number of rows
		dimension,						// number of columns
		nnz, values, colInd, rowPtr,	// matrix data
		SLU_NR,							// SLU_NC - non supernodal column matrix
		SLU_D,							// SLU_D - Storage for the "double" type
		SLU_GE);						// general matrix. No special type

	// create a fake RHS vector
	double* fakeRhsData = new double[dimension];
	for(int i = 0; i < dimension; ++i)
		fakeRhsData[i] = 1;
	SuperMatrix fakeRhs;
	dCreate_Dense_Matrix(&fakeRhs, dimension, 1, fakeRhsData, dimension, SLU_DN, SLU_D, SLU_GE);
	
	// prepare for factorization
	SuperLUStat_t stat;
	superlu_options_t opts;	
	int info;
	permc = new int[dimension];
	permr = new int[dimension];
	L = new SuperMatrix;
	U = new SuperMatrix;

	set_default_options(&opts);
	opts.ColPerm = NATURAL;
	StatInit(&stat);

	// TODO: Write the factorization code
	dgssv(
		&opts,
		&A,
		permc,
		permr,
		L,
		U,
		&fakeRhs,
		&stat,
		&info);

	delete [] fakeRhsData;
	Destroy_SuperMatrix_Store(&fakeRhs);
	Destroy_CompRow_Matrix(&A);
	StatFree(&stat);
	
	if (info != 0)
	{
		Destroy_SuperNode_Matrix(L);
		Destroy_CompCol_Matrix(U);
		delete L;
		delete U;
		delete [] permc;
		delete [] permr;

		throw gcnew InvalidOperationException("LU Factorization failed!");
	}
}

NSuperLU::FactoredSparseMatrix::~FactoredSparseMatrix()
{
	Destroy_SuperNode_Matrix(L);
	Destroy_CompCol_Matrix(U);
	delete L;
	delete U;
	delete [] permc;
	delete [] permr;
}

array<double>^ NSuperLU::FactoredSparseMatrix::Solve(array<double>^ rhsArray)
{
	int dimension = rhsArray->Length;
	double* rhsData = new double[dimension];
	for(int i = 0; i < dimension; ++i)
		rhsData[i] = rhsArray[i];

	// build the RHS matrix
	SuperMatrix rhsMatrix;
	dCreate_Dense_Matrix(&rhsMatrix, dimension, 1, rhsData, dimension, SLU_DN, SLU_D, SLU_GE);

	// prepare to solve
	SuperLUStat_t stat;
	StatInit(&stat);
	int info;

	// solve the system
	dgstrs(
		NOTRANS, // NOTRANS - solve AX = B and not A'X = B
		L,
		U,
		permc,
		permr,
		&rhsMatrix,
		&stat,
		&info);

	// delete memory allocated for the statistics
	StatFree(&stat);

	if (info != 0)
	{
		delete [] rhsData;
		Destroy_SuperMatrix_Store(&rhsMatrix);

		throw gcnew InvalidOperationException("Solution failed");
	}
	else
	{
		array<double>^ result = gcnew array<double>(dimension);
		for(int i = 0; i < dimension; ++i)
			result[i] = rhsData[i];

		delete [] rhsData;
		Destroy_SuperMatrix_Store(&rhsMatrix);

		return result;
	}
}