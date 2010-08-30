// MatchPoints.Wrapper.h

#pragma once

#include "matrix.h"
#include "point_matcher.h"

using namespace System;

namespace MatchPointsWrapper {

	public ref class PointMatcher
	{
	public:
		static array<int, 2>^ MatchPoints(array<double, 2>^ w)
		{
			int r = w->GetLength(0);
			int c = w->GetLength(1);

			Matrix<double> wMatrix(r, c);
			for(int i = 0; i < r; ++i)
				for(int j = 0; j < c; ++j)
					wMatrix(i, j) = w[i, j];

			Matrix<int> matchesFound = find_matches(wMatrix);

			array<int, 2>^ result = gcnew array<int, 2>(r, c);
			for(int i = 0; i < r; ++i)
				for(int j = 0; j < c; ++j)
					result[i, j] = matchesFound(i, j);
			return result;
		}
	};
}
