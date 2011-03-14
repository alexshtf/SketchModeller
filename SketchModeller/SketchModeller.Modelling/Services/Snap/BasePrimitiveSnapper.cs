﻿using System;
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

        protected UiState UiState
        {
            get { return uiState; }
        }

        protected SessionData SessionData
        {
            get { return sessionData; }
        }

        protected IEnumerable<Term> ProjectionFit(SnappedPointsSet item)
        {
            const int SAMPLE_SIZE = 10;
            var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
            return ProjectionFit(item, sample);
        }

        /// <summary>
        /// Generates a set of terms, one for each given 2D point, that measure the fitness of each point to being
        /// a 2D projection of the given set.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="sample">The set of 2D points</param>
        /// <returns>The set of terms, one for each point in <paramref name="sample"/> that measures the fitness of each such point
        /// to the set in <paramref name="pointsSet"/>.</returns>
        protected IEnumerable<Term> ProjectionFit(SnappedPointsSet pointsSet, Point[] sample)
        {
            var terms =
                from point in sample
                select ProjectionFit(pointsSet, point);

            return terms;
        }

        /// <summary>
        /// Generates a term that gets smaller as the given 2D point fits a 3D points set projection.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="point">The 2D point</param>
        /// <returns>The term that measures fitness of <paramref name="point"/> being on the 2D projection of the set specified by <paramref name="pointsSet"/></returns>
        protected Term ProjectionFit(SnappedPointsSet pointsSet, Point point)
        {
            // here we explicitly assume that the view vector is (0, 0, 1) or (0, 0, -1)
            var x_ = point.X;
            var y_ = point.Y;

            var cx = pointsSet.Center.X;
            var cy = pointsSet.Center.Y;
            var cz = pointsSet.Center.Z;

            var nx = pointsSet.Axis.X;
            var ny = pointsSet.Axis.Y;
            var nz = pointsSet.Axis.Z;

            var r = pointsSet.Radius;

            var dx = cx - x_;
            var dy = cy + y_;

            var lhs = TermBuilder.Sum(
                TermBuilder.Power(dx * nz, 2),
                TermBuilder.Power(dy * nz, 2),
                TermBuilder.Power(dx * nx + dy * ny, 2));
            var rhs = TermBuilder.Power(r * nz, 2);

            return TermBuilder.Power(lhs - rhs, 2);
        }

        protected Point3D ProjectViewDirectionOnPlane(Plane3D plane, Point3D point)
        {
            var sndPoint = point + uiState.SketchPlane.Normal;
            var t = plane.IntersectLine(point, sndPoint);
            return MathUtils3D.Lerp(point, sndPoint, t);
        }
    }
}
