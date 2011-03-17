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

        protected UiState UiState
        {
            get { return uiState; }
        }

        protected SessionData SessionData
        {
            get { return sessionData; }
        }

        protected Point3D ProjectViewDirectionOnPlane(Plane3D plane, Point3D point)
        {
            var sndPoint = point + uiState.SketchPlane.Normal;
            var t = plane.IntersectLine(point, sndPoint);
            return MathUtils3D.Lerp(point, sndPoint, t);
        }
    }
}
