using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;
using SketchModeller.Modelling.Computations;
using System.Windows;

using Enumerable = System.Linq.Enumerable;
using TermUtils = SketchModeller.Utilities.TermUtils;
using System.Windows.Media.Media3D;

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
            
                AxisResult = newPrimitive.Axis.Value.Normalized(),
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
            var silhouettesCount = 
                (new PointsSequence[] { snappedPrimitive.LeftSilhouette, snappedPrimitive.RightSilhouette })
                .Count(curve => curve != null);

            var featuresCount = snappedPrimitive.FeatureCurves.Count(curve => curve.SnappedTo != null);

            // get annotated feature curves of this primitive.
            var annotated = new HashSet<FeatureCurve>(curvesToAnnotations.Keys.Where(key => curvesToAnnotations[key].Count > 0));
            annotated.Intersect(snappedPrimitive.FeatureCurves);
        
            Tuple<Term, Term[]> result = null;

            if (silhouettesCount == 2 && featuresCount == 2)
                result = FullInfo(snappedPrimitive);
            else if (silhouettesCount == 1 && featuresCount == 2)
                result = SingleSilhouetteTwoFeatures(snappedPrimitive, annotated);
            else if (silhouettesCount == 2 && featuresCount == 1)
                result = SingleFeatureTwoSilhouettes(snappedPrimitive, annotated);

            return result;
        }

        private Tuple<Term, Term[]> SingleFeatureTwoSilhouettes(SnappedStraightGenCylinder snappedPrimitive, HashSet<FeatureCurve> annotated)
        {
            var leftPts = snappedPrimitive.LeftSilhouette.Points;
            var rightPts = snappedPrimitive.RightSilhouette.Points;
            var pointsProgress = snappedPrimitive.Components.Select(x => x.Progress).ToArray();

            Point[] featureCurve;
            if (snappedPrimitive.TopFeatureCurve.SnappedTo != null)
                featureCurve = snappedPrimitive.TopFeatureCurve.SnappedTo.Points;
            else
                featureCurve = snappedPrimitive.BottomFeatureCurve.SnappedTo.Points;

            var ellipse = EllipseFitter.Fit(featureCurve);
            var orientationBasis = EllipseHelper.CircleOrientation(ellipse);
            var approxOrientation = GetOrientation(orientationBasis, snappedPrimitive.AxisResult);

            var spine = StraightSpine.Compute(leftPts, rightPts, pointsProgress, new Vector(approxOrientation.X, -approxOrientation.Y));
            var radii = spine.Item1;
            var spineStart = spine.Item2;
            var spineEnd = spine.Item3;

            return CreateSGCTerms(snappedPrimitive,  approxOrientation, radii, spineStart, spineEnd);
        }

        private Tuple<Term, Term[]> SingleSilhouetteTwoFeatures(SnappedStraightGenCylinder snappedPrimitive, HashSet<FeatureCurve> annotated)
        {
            var pts = 
                snappedPrimitive.LeftSilhouette != null 
                ? snappedPrimitive.LeftSilhouette.Points
                : snappedPrimitive.RightSilhouette.Points;

            var pointsProgress = 
                snappedPrimitive.Components.Select(x => x.Progress).ToArray();

            // compute the term we get from the feature curves. used mainly to optimize
            // for the axis orientation
            var topEllipse = EllipseFitter.Fit(snappedPrimitive.TopFeatureCurve.SnappedTo.Points);
            var botEllipse = EllipseFitter.Fit(snappedPrimitive.BottomFeatureCurve.SnappedTo.Points);
          
            var approxOrientation = GetOrientation(topEllipse, botEllipse, snappedPrimitive.AxisResult);

            var spine = StraightSpine.Compute(pts, pointsProgress, topEllipse.Center, botEllipse.Center - topEllipse.Center);
           
            var radii = spine.Item1;
            var spineStart = spine.Item2;
            var spineEnd = spine.Item3;

            return CreateSGCTerms(snappedPrimitive, approxOrientation, radii, spineStart, spineEnd);
}

        private Tuple<Term, Term[]> FullInfo(SnappedStraightGenCylinder snappedPrimitive)
        {
            var leftPts = snappedPrimitive.LeftSilhouette.Points;
            var rightPts = snappedPrimitive.RightSilhouette.Points;

            var pointsProgress = 
                snappedPrimitive.Components.Select(x => x.Progress).ToArray();

            // compute the term we get from the feature curves. used mainly to optimize
            // for the axis orientation
            var topEllipse = EllipseFitter.Fit(snappedPrimitive.TopFeatureCurve.SnappedTo.Points);
            var botEllipse = EllipseFitter.Fit(snappedPrimitive.BottomFeatureCurve.SnappedTo.Points);

            var approxOrientation = GetOrientation(topEllipse, botEllipse, snappedPrimitive.AxisResult);

            // compute the spine of the primitive
            var spine = StraightSpine.Compute(leftPts, rightPts, pointsProgress, topEllipse.Center, botEllipse.Center - topEllipse.Center);

            var radii = spine.Item1;
            var spineStart = spine.Item2;
            var spineEnd = spine.Item3;

            return CreateSGCTerms(snappedPrimitive, approxOrientation, radii, spineStart, spineEnd);
        }

        private static Tuple<Term, Term[]> CreateSGCTerms(SnappedStraightGenCylinder snappedPrimitive, Vector3D approxOrientation, double[] radii, Point spineStart, Point spineEnd)
        {
            var orientationTerm =
                TermBuilder.Power(approxOrientation.X - snappedPrimitive.Axis.X, 2) +
                TermBuilder.Power(approxOrientation.Y - snappedPrimitive.Axis.Y, 2) +
                TermBuilder.Power(-approxOrientation.Z - snappedPrimitive.Axis.Z, 2);

            // the difference between the primitive's radii and the computed radii is minimized
            var radiiApproxTerm = TermUtils.SafeAvg(
                from i in Enumerable.Range(0, snappedPrimitive.Components.Length)
                let component = snappedPrimitive.Components[i]
                let radius = radii[i]
                select TermBuilder.Power(component.Radius - radius, 2));

            // the smoothness of the primitive's radii (laplacian norm) is minimized
            var radiiSmoothTerm = TermUtils.SafeAvg(
                from pair in snappedPrimitive.Components.SeqTripples()
                let r1 = pair.Item1.Radius
                let r2 = pair.Item2.Radius
                let r3 = pair.Item3.Radius
                select TermBuilder.Power(r2 - 0.5 * (r1 + r3), 2)); // how far is r2 from the avg of r1 and r3

            // start/end points should be as close as possible to the bottom/top centers
            var startTerm = 0.5 * (
                TermBuilder.Power(snappedPrimitive.BottomCenter.X - spineStart.X, 2) +
                TermBuilder.Power(snappedPrimitive.BottomCenter.Y + spineStart.Y, 2));

            var topCenter = snappedPrimitive.GetTopCenter();
            var endTerm = 0.5 * (
                TermBuilder.Power(topCenter.X - spineEnd.X, 2) +
                TermBuilder.Power(topCenter.Y + spineEnd.Y, 2));

            // we specifically wish to give higher weight to first and last radii, so we have
            // an additional first/last radii term.
            var endpointsRadiiTerm =
                TermBuilder.Power(radii[0] - snappedPrimitive.ComponentResults[0].Radius, 2) +
                TermBuilder.Power(radii.Last() - snappedPrimitive.Components.Last().Radius, 2);

            // objective - weighed average of all terms
            var objective =
                radiiApproxTerm +
                radiiSmoothTerm +
                startTerm +
                endTerm +
                orientationTerm +
                endpointsRadiiTerm;

            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };
            return Tuple.Create(objective, constraints);
        }

        private Vector3D GetOrientation(
            EllipseParams topEllipse, 
            EllipseParams botEllipse,
            Vector3D axisApproximation)
        {
            var topCircleBasis = EllipseHelper.CircleOrientation(topEllipse);
            var botCircleBasis = EllipseHelper.CircleOrientation(botEllipse);

            var topOrientation = GetOrientation(topCircleBasis, axisApproximation);
            var botOrientation = GetOrientation(botCircleBasis, axisApproximation);

            if (Vector3D.DotProduct(botOrientation, topOrientation) < 0)
                botOrientation = -botOrientation;

            var topPerimeter = EllipseHelper.ApproxPerimeter(topEllipse.XRadius, topEllipse.YRadius);
            var botPerimeter = EllipseHelper.ApproxPerimeter(botEllipse.XRadius, botEllipse.YRadius);

            //return result;
            if (topPerimeter > botPerimeter)
                return topOrientation;
            else
                return botOrientation;
        }

        private Vector3D GetOrientation(Tuple<Vector3D, Vector3D> circleBasis, Vector3D axisApproximation)
        {
            var normal1 = Vector3D.CrossProduct(circleBasis.Item1, circleBasis.Item2);
            normal1.Normalize();

            var normal2 = new Vector3D(normal1.X, normal1.Y, -normal1.Z);

            var normal1Difference = (normal1 - axisApproximation).LengthSquared;
            var normal2Difference = (normal2 - axisApproximation).LengthSquared;

            if (normal1Difference < normal2Difference)
                return normal1;
            else
                return normal2;
        }

        private Term FeaturesTerm(IEnumerable<CircleFeatureCurve> iEnumerable)
        {
            var terms =
                from item in iEnumerable
                from term in ProjectionFit.Compute(item)
                select term;

            var objective = TermUtils.SafeAvg(terms);

            return objective;
        }
    }
}
