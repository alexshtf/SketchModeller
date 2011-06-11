using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;

namespace SketchModeller.Modelling.Services.Snap
{
    class SgcSnapper : BasePrimitivesSnapper<NewStraightGenCylinder, SnappedStraightGenCylinder>
    {
        protected override SnappedStraightGenCylinder Create(PointsSequence[] selectedCurves, NewStraightGenCylinder newPrimitive)
        {
            var snappedPrimitive = InitNewSnapped(newPrimitive);
            snappedPrimitive.SnappedTo =
                newPrimitive.AllCurves
                .Select(c => c.AssignedTo)
                .Where(c => c != null)
                .ToArray();

            snappedPrimitive.TopFeatureCurve.SnappedTo =
                newPrimitive.TopCircle.AssignedTo;

            snappedPrimitive.BottomFeatureCurve.SnappedTo =
                newPrimitive.BottomCircle.AssignedTo;

            snappedPrimitive.LeftSilhouette =
                newPrimitive.LeftSilhouette.AssignedTo;

            snappedPrimitive.RightSilhouette =
                newPrimitive.RightSilhouette.AssignedTo;

            return snappedPrimitive;
        }

        #region Creation related methods

        private SnappedStraightGenCylinder InitNewSnapped(NewStraightGenCylinder newPrimitive)
        {
            var result = new SnappedStraightGenCylinder
            {
                Axis = SnapperHelper.GenerateVarVector(),
                BottomCenter = SnapperHelper.GenerateVarVector(),
                Length = new Variable(),
                Components = GenerateComponents(newPrimitive.Components),
            
                AxisResult = newPrimitive.Axis.Normalized(),
                BottomCenterResult = newPrimitive.Bottom,
                LengthResult = newPrimitive.Length,
                ComponentResults = newPrimitive.Components.CloneArray(),
            };

            return result;
        }

        private SnappedCyliderComponent[] GenerateComponents(CylinderComponent[] cylinderComponents)
        {
            var n = cylinderComponents.Length;
            var result = new SnappedCyliderComponent[n];
            for (int i = 0; i < n; ++i)
                result[i] = new SnappedCyliderComponent(new Variable(), cylinderComponents[i].Progress);
            return result;
        }

        #endregion

        protected override Tuple<Term, Term[]> Reconstruct(SnappedStraightGenCylinder snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            var topCurve = snappedPrimitive.TopFeatureCurve.SnappedTo;
            var botCurve = snappedPrimitive.BottomFeatureCurve.SnappedTo; ;
            var silhouettes = new PointsSequence[] { snappedPrimitive.LeftSilhouette, snappedPrimitive.RightSilhouette };

            // get annotated feature curves of this primitive.
            var annotated = new HashSet<FeatureCurve>(curvesToAnnotations.Keys.Where(key => curvesToAnnotations[key].Count > 0));
            annotated.Intersect(snappedPrimitive.FeatureCurves);
        
            Tuple<Term, Term[]> result = null;
            if (topCurve != null && botCurve != null)
                result = FullInfo(snappedPrimitive);
            else if (silhouettes.Length == 2)
            {
                if (!(topCurve == null && botCurve == null))
                    result = TwoSilhouettesSingleFeature(snappedPrimitive, annotated);
                else
                    result = TwoSilhouettesNoFeatures(snappedPrimitive, annotated);
            }

            return result;
        }

        private Tuple<Term, Term[]> FullInfo(SnappedStraightGenCylinder snappedPrimitive)
        {
            throw new NotImplementedException();
        }

        private Tuple<Term, Term[]> TwoSilhouettesNoFeatures(SnappedStraightGenCylinder snappedPrimitive, HashSet<FeatureCurve> annotated)
        {
            throw new NotImplementedException();
        }

        private Tuple<Term, Term[]> TwoSilhouettesSingleFeature(SnappedStraightGenCylinder snappedPrimitive, HashSet<FeatureCurve> annotated)
        {
            throw new NotImplementedException();
        }
    }
}
