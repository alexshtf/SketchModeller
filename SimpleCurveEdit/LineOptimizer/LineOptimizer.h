// LineOptimizer.h

#pragma once

#include "OptimizationPoint.h"

using namespace System;
using namespace System::Windows::Media::Media3D;

namespace LineOptimizer 
{
	public ref class LineOptimizer
	{
	private:
		array<OptimizationPoint ^> ^ before;
		array<OptimizationPoint ^> ^ after;
		array<OptimizationPoint ^> ^ middle;
		Matrix3D projectionTransform;
	public:
		LineOptimizer(
			array<OptimizationPoint ^> ^ _before, 
			array<OptimizationPoint ^> ^ _after, 
			array<OptimizationPoint ^> ^ _middle, 
			Matrix3D _projectionTransform)
			: before(_before)
			, after(_after)
			, middle(_middle)
			, projectionTransform(_projectionTransform)
		{
		}

		void Solve();
	};
}
