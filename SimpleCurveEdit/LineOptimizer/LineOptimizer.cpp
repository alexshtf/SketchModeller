// This is the main DLL file.


#include "stdafx.h"
#include "Sparse"
#include "LineOptimizer.h"

using namespace Eigen;
using namespace System::Windows;

namespace LineOptimizer
{
	void LineOptimizer::Solve()
	{
		int k = before->Length;
		int l = middle->Length;
		int m = after->Length;

		int totalCount = k + l + m;

		// we will describe hard projection constraints in matrix form Px = y (x is the vector of ALL points)
		VectorXd y(2 * l);
		DynamicSparseMatrix<double> P(2 * l, totalCount);

#define declare_p(idx) double p##idx = projectionTransform.M##idx;

		declare_p(11); declare_p(12); declare_p(13); declare_p(14);
		declare_p(21); declare_p(22); declare_p(23); declare_p(24);
		declare_p(31); declare_p(32); declare_p(33); declare_p(34);
		/*declare_p(41); declare_p(42); declare_p(43); */ declare_p(44);
		double p41 = projectionTransform.OffsetX;
		double p42 = projectionTransform.OffsetY;
		double p43 = projectionTransform.OffsetZ;

		for(int i = 0; i < middle->Length; ++i)
		{
			double u = middle[i]->ProjConstraint.X;
			double v = middle[i]->ProjConstraint.Y;
			
			int row = 2 * i;
			int col = 3*k + 3*l;

			y(row) = u * p44 - p41;
			y(row + 1) = v * p44 - p42;

			P(row + 0, col + 0) = p11 - u * p14;
			P(row + 0, col + 1) = p21 - u * p24;
			P(row + 0, col + 2) = p31 - u * p34;
			P(row + 1, col + 0) = p12 - v * p14;
			P(row + 1, col + 1) = p22 - v * p24;
			P(row + 1, col + 2) = p32 - v * p34;
		}

		// we will describe our soft constraint and regularization operators as the matrix L
	}
}