#pragma once

using namespace System;
using namespace System::Windows;
using namespace System::Windows::Media;
using namespace System::Windows::Media::Media3D;

namespace LineOptimizer 
{
	public ref class OptimizationPoint
	{
	public:
		property Point3D Original;
		property Point3D New;
		property Point ProjConstraint;
	};
}