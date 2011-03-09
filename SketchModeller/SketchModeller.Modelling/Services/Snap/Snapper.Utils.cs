using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using AutoDiff;
using SketchModeller.Infrastructure.Data;
using System.Windows;
using SketchModeller.Utilities;
using System.Diagnostics.Contracts;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Services.Snap
{
	public partial class NewSnapper
	{
		private static Term MeanSquaredError(Term[] terms, double[] values)
		{
			Contract.Requires(terms != null);
			Contract.Requires(values != null);
			Contract.Requires(terms.Length == values.Length);
			Contract.Requires(Contract.ForAll(terms, term => term != null));
			Contract.Ensures(Contract.Result<Term>() != null);

			var errorTerms = from i in Enumerable.Range(0, terms.Length)
							 select TermBuilder.Power(terms[i] + (-values[i]), 2);

			return (1 / (double)terms.Length) * TermUtils.SafeSum(errorTerms);
		}

		private IEnumerable<Term> ProjectionConstraints(SnappedPointsSet item)
		{
			const int SAMPLE_SIZE = 10;
			var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
			var terms =
				from point in sample
				from term in ProjectionConstraint(item, point)
				select term;

			return terms;
		}

		private IEnumerable<Term> ProjectionConstraint(SnappedPointsSet item, Point point)
		{
			// here we explicitly assume that the view vector is (0, 0, 1) or (0, 0, -1)
			var x_ = point.X;
			var y_ = point.Y;

			var cx = item.Center.X;
			var cy = item.Center.Y;
			var cz = item.Center.Z;

			var nx = item.Axis.X;
			var ny = item.Axis.Y;
			var nz = item.Axis.Z;

			var r = item.Radius;

			var dx = cx - x_;
			var dy = cy + y_;

			var lhs = TermBuilder.Sum(
				TermBuilder.Power(dx * nz, 2),
				TermBuilder.Power(dy * nz, 2),
				TermBuilder.Power(dx * nx + dy * ny, 2));
			var rhs = TermBuilder.Power(r * nz, 2);

			yield return TermBuilder.Power(lhs - rhs, 2);
		}

		private double EstimateRadius(Point3D center, IEnumerable<Point3D> proj)
		{
			var radii = proj.Select(x => (center - x).Length);
			return radii.Average();
		}

		private Point3D ProjectOnPlane(Plane3D plane, Point3D point)
		{
			var sndPoint = point + uiState.SketchPlane.Normal;
			var t = plane.IntersectLine(point, sndPoint);
			return MathUtils3D.Lerp(point, sndPoint, t);
		}
	}
}
