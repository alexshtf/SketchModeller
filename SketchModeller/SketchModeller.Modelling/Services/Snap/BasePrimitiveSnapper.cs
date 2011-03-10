using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Utilities;
using System.Windows;
using System.Windows.Media.Media3D;
using Utils;

namespace SketchModeller.Modelling.Services.Snap
{
    abstract class BasePrimitivesSnapper<TNew, TSnapped> : IPrimitiveSnapper
        where TNew : NewPrimitive
        where TSnapped : SnappedPrimitive
    {
        private UiState uiState;
        private SessionData sessionData;

        public void Initialize(UiState uiState, SessionData sessionData)
        {
            this.uiState = uiState;
            this.sessionData = sessionData;
        }

        public SnappedPrimitive Create(PointsSequence[] selectedCurves, NewPrimitive newPrimitive)
        {
            TNew concrete = (TNew)newPrimitive;
            return Create(selectedCurves, concrete);
        }

        public Tuple<Term, Term[]> Reconstruct(SnappedPrimitive snappedPrimitive)
        {
            return Reconstruct((TSnapped)snappedPrimitive);
        }

        public Type NewPrimitiveType
        {
            get { return typeof(TNew); }
        }


        public Type SnappedPrimitiveType
        {
            get { return typeof(TSnapped); }
        }

        protected abstract TSnapped Create(PointsSequence[] selectedCurves, TNew newPrimitive);
        protected abstract Tuple<Term, Term[]> Reconstruct(TSnapped snappedPrimitive);


        protected IEnumerable<Term> ProjectionConstraints(SnappedPointsSet item)
        {
            const int SAMPLE_SIZE = 10;
            var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
            var terms =
                from point in sample
                from term in ProjectionConstraint(item, point)
                select term;

            return terms;
        }

        protected IEnumerable<Term> ProjectionConstraint(SnappedPointsSet item, Point point)
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

        protected Point3D ProjectOnPlane(Plane3D plane, Point3D point)
        {
            var sndPoint = point + uiState.SketchPlane.Normal;
            var t = plane.IntersectLine(point, sndPoint);
            return MathUtils3D.Lerp(point, sndPoint, t);
        }
    }
}
