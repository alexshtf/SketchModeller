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
    class BgcSnapper : BasePrimitivesSnapper<NewBendedGenCylinder, SnappedBendedGenCylinder>
    {
        protected override SnappedBendedGenCylinder Create(NewBendedGenCylinder newPrimitive)
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

        //#region Creation related methods

        private SnappedBendedGenCylinder InitNewSnapped(NewBendedGenCylinder newPrimitive)
        {
            var result = new SnappedBendedGenCylinder
            {
                //TopCenter = SnapperHelper.GenerateVarVector(),
                BottomCenter = SnapperHelper.GenerateVarVector(),
                NPtop = new TVec(new Variable(), new Variable()), 
                NPbot = new TVec(new Variable(), new Variable()),
                U = SnapperHelper.GenerateVarVector(),
                V = SnapperHelper.GenerateVarVector(),
                Components = GenerateComponents(newPrimitive.Components), 

                Uresult = newPrimitive.Uview,
                Vresult = newPrimitive.Vview,
                NPtopResult = new Vector(1, 0),
                NPbotResult = new Vector(0, 1),
                TopCenterResult = newPrimitive.Top,
                BottomCenterResult = newPrimitive.Bottom,
                ComponentResults = newPrimitive.Components.CloneArray(),
            };

            return result;
        }

        private SnappedBendedCylinderComponent[] GenerateComponents(BendedCylinderComponent[] cylinderComponents)
        {
            var n = cylinderComponents.Length;
            var result = new SnappedBendedCylinderComponent[n];
            for (int i = 0; i < n; ++i)
                result[i] = new SnappedBendedCylinderComponent(new Variable(), cylinderComponents[i].Progress, new Variable(), new Variable());
            return result;
        }

        protected override Tuple<Term, Term[]> Reconstruct(SnappedBendedGenCylinder snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            var silhouettesCount =
                (new PointsSequence[] { snappedPrimitive.LeftSilhouette, snappedPrimitive.RightSilhouette })
                .Count(curve => curve != null);

            var featuresCount = snappedPrimitive.FeatureCurves
                .Cast<CircleFeatureCurve>()
                .Count(curve => curve.SnappedTo != null);

            // get annotated feature curves of this primitive.
            var annotated = new HashSet<FeatureCurve>(curvesToAnnotations.Keys.Where(key => curvesToAnnotations[key].Count > 0));
            annotated.Intersect(snappedPrimitive.FeatureCurves);
            Tuple<Term, Term[]> result = null;

            if (silhouettesCount == 2 && featuresCount == 2)
            {
                //MessageBox.Show("Inside Full Feature Optimization");
                result = FullInfo(ref snappedPrimitive);
            }
            else
            {
                //MessageBox.Show("Sorry not applicable");
            }
            /*else if (silhouettesCount == 1 && featuresCount == 2)
                result = SingleSilhouetteTwoFeatures(snappedPrimitive, annotated);
            else if (silhouettesCount == 2 && featuresCount == 1)
                result = SingleFeatureTwoSilhouettes(snappedPrimitive, annotated);*/

            return result;
        }

        private Tuple<Term, Term[]> FullInfo(ref SnappedBendedGenCylinder snappedPrimitive)
        {
            var leftPts = snappedPrimitive.LeftSilhouette.Points;
            var rightPts = snappedPrimitive.RightSilhouette.Points;

            var pointsProgress =
                snappedPrimitive.Components.Select(x => x.Progress).ToArray();

            // compute the term we get from the feature curves. used mainly to optimize
            // for the axis orientation
            var botEllipse = EllipseFitter.Fit(snappedPrimitive.BottomFeatureCurve.SnappedTo.Points);
            var topEllipse = EllipseFitter.Fit(snappedPrimitive.TopFeatureCurve.SnappedTo.Points);
            var spine = BendedSpine.Compute(leftPts, rightPts, pointsProgress);
            var radii = spine.Item2;
            var medialAxis = spine.Item1;
            bool reverse = false;
            Vector vnormal = new Vector(botEllipse.Center.X - medialAxis[0].X, -botEllipse.Center.Y + medialAxis[0].Y);
            Vector vreverse = new Vector(botEllipse.Center.X - medialAxis[medialAxis.Length - 1].X, -botEllipse.Center.Y + medialAxis[medialAxis.Length - 1].Y);
            if (vnormal.Length > vreverse.Length) reverse = true;
            if (reverse)
            {
                int left = 0;
                int right = medialAxis.Length - 1;
                while (left < right)
                {
                    swap(ref radii[left], ref radii[right]);
                    swap(ref medialAxis[left], ref medialAxis[right]);
                    left++;
                    right--;
                }
            }
            return CreateBGCTerms(snappedPrimitive, radii, medialAxis);
        }

        private static void swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        private static Tuple<Term, Term[]> CreateBGCTerms(SnappedBendedGenCylinder snappedPrimitive, double[] radii, Point[] medial_axis)
        {
            var terms =
               from item in snappedPrimitive.FeatureCurves.Cast<CircleFeatureCurve>()
               where item.SnappedTo != null
               from term in ProjectionFit.Compute(item)
               select term;
            var orientationTerm = TermUtils.SafeAvg(terms);

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

            var positionSmoothnessTerm = TermUtils.SafeAvg(
                from triple in snappedPrimitive.Components.SeqTripples()
                let p1 = new TVec(triple.Item1.vS, triple.Item1.vT)
                let p2 = new TVec(triple.Item2.vS, triple.Item2.vT)
                let p3 = new TVec(triple.Item3.vS, triple.Item3.vT)
                let laplacian = p2 - 0.5 * (p1 + p3)
                select laplacian.NormSquared);
   
            Term[] TermArray = new Term[medial_axis.Length];
            for (int i = 0; i < medial_axis.Length; i++)
            {
                TVec PointOnSpine = snappedPrimitive.BottomCenter + snappedPrimitive.Components[i].vS * snappedPrimitive.U + snappedPrimitive.Components[i].vT * snappedPrimitive.V; 
                TermArray[i] = TermBuilder.Power(PointOnSpine.X - medial_axis[i].X, 2) +
                               TermBuilder.Power(PointOnSpine.Y + medial_axis[i].Y, 2);
            }
            var spinePointTerm = TermUtils.SafeAvg(TermArray);

            // we specifically wish to give higher weight to first and last radii, so we have
            // an additional first/last radii term.
            var endpointsRadiiTerm =
                TermBuilder.Power(radii[0] - snappedPrimitive.Components.First().Radius, 2) +
                TermBuilder.Power(radii.Last() - snappedPrimitive.Components.Last().Radius, 2);

            // objective - weighed average of all terms
            var objective =
                0.1 * orientationTerm +
                radiiApproxTerm +
                radiiSmoothTerm +
                positionSmoothnessTerm +
                spinePointTerm +
                endpointsRadiiTerm;

            var sB = snappedPrimitive.NPbot.X;
            var tB = snappedPrimitive.NPbot.Y;

            var s0 = snappedPrimitive.Components[0].vS;
            var t0 = snappedPrimitive.Components[0].vT;
            var s1 = snappedPrimitive.Components[1].vS;
            var t1 = snappedPrimitive.Components[1].vT;
            var botNormalParallelism = tB * (s1 - s0) - sB * (t1 - t0);

            var sT = snappedPrimitive.NPtop.X;
            var tT = snappedPrimitive.NPtop.Y;
            var sLast = snappedPrimitive.Components[snappedPrimitive.Components.Length - 1].vS;
            var tLast = snappedPrimitive.Components[snappedPrimitive.Components.Length - 1].vT;
            var sBeforeLast = snappedPrimitive.Components[snappedPrimitive.Components.Length - 2].vS;
            var tBeforeLast = snappedPrimitive.Components[snappedPrimitive.Components.Length - 2].vT;
            var topNormalParallelism = tT * (sLast - sBeforeLast) * sT * (tLast - tBeforeLast);

            var topNormalized = snappedPrimitive.NPtop.NormSquared - 1;
            var botNormalized = snappedPrimitive.NPbot.NormSquared - 1;
            var uNormalized = snappedPrimitive.U.NormSquared - 1;
            var vNormalized = snappedPrimitive.V.NormSquared - 1;
            var uvOrthogonal = TVec.InnerProduct(snappedPrimitive.U, snappedPrimitive.V);
            var firstComponentBottomS = snappedPrimitive.Components[0].vS;
            var firstComponentBottomT = snappedPrimitive.Components[0].vT;
            var constraints = new Term[] 
            { 
                topNormalized, 
                botNormalized, 
                uNormalized, 
                vNormalized, 
                uvOrthogonal, 
                firstComponentBottomS, 
                firstComponentBottomT,
                botNormalParallelism,
                topNormalParallelism
            };

            return Tuple.Create(objective, constraints);
        }

        private Vector3D NewGetOrientation(
                Point[] topPnts,
                Point[] botPnts,
                EllipseParams topEllipse,
                EllipseParams botEllipse,
                Point[] spineApproximation,
                ref bool Reverse)
        {
            /*MessageBox.Show(String.Format("Ellipse Center : ({0},{1})", botEllipse.Center.X, botEllipse.Center.Y));
            MessageBox.Show(String.Format("Ellipse Center : ({0},{1})", spineApproximation[0].X, -spineApproximation[0].Y));
            MessageBox.Show(String.Format("Ellipse Center : ({0},{1})", spineApproximation[spineApproximation.Length - 1].X, -spineApproximation[spineApproximation.Length - 1].Y));*/
            int nspine = spineApproximation.Length;
            // Here we check whether the bottom feature curve is the start of the computed spine or the spine is reversed 
            Vector vnormal = new Vector(botEllipse.Center.X - spineApproximation[0].X, botEllipse.Center.Y + spineApproximation[0].Y);
            Vector vreverse = new Vector(botEllipse.Center.X - spineApproximation[spineApproximation.Length - 1].X, botEllipse.Center.Y + spineApproximation[spineApproximation.Length - 1].Y);

            Reverse = false;
            if (vnormal.Length > vreverse.Length) Reverse = true;
            Vector botAxis = new Vector();
            Vector topAxis = new Vector();
            if (!Reverse)
            {
                botAxis = new Vector(spineApproximation[1].X - spineApproximation[0].X, spineApproximation[0].Y - spineApproximation[1].Y);
                topAxis = new Vector(spineApproximation[nspine - 1].X - spineApproximation[nspine - 2].X, spineApproximation[nspine - 2].Y - spineApproximation[nspine - 1].Y);
            }
            else
            {
                topAxis = new Vector(spineApproximation[0].X - spineApproximation[1].X, spineApproximation[1].Y - spineApproximation[0].Y);
                botAxis = new Vector(spineApproximation[nspine - 2].X - spineApproximation[nspine - 1].X, spineApproximation[nspine - 1].Y - spineApproximation[nspine - 2].Y);
            }
            //MessageBox.Show(String.Format("Bottom Vector:{0}, {1}", botAxis.X, botAxis.Y));
            //MessageBox.Show(String.Format("Top Vector:{0}, {1}", topAxis.X, topAxis.Y));
            double mx = 0.0;
            double my = 0.0;
            foreach (Point pnt in topPnts)
            {
                mx += pnt.X;
                my += pnt.Y;
            }
            mx /= topPnts.Length;
            my /= topPnts.Length;
            double Cxx = 0.0;
            double Cyy = 0.0;
            double Cxy = 0.0;
            foreach (Point pnt in topPnts)
            {
                Cxx += Math.Pow(pnt.X - mx, 2.0);
                Cyy += Math.Pow(pnt.Y - my, 2.0);
                Cxy += (pnt.X - mx) * (pnt.Y - my);
            }
            Cxx /= topPnts.Length;
            Cyy /= topPnts.Length;
            Cxy /= topPnts.Length;
            double trC = Cxx + Cyy;
            double detC = Cxx * Cyy - Cxy * Cxy;
            double l1 = 0.5 * trC + Math.Sqrt(0.25 * trC * trC - detC);
            double l2 = 0.5 * trC - Math.Sqrt(0.25 * trC * trC - detC);

            double a1 = 2 * Math.Sqrt(l1);
            double a2 = 2 * Math.Sqrt(l2);
            Vector3D ApproxOrientationTop = new Vector3D(Math.Sqrt(a1 * a1 - 4 * Cxx) / a1, Math.Sqrt(a1 * a1 - 4 * Cyy) / a1, a2 / a1);
            Vector ApproxOrientationTopProj = new Vector(ApproxOrientationTop.X, ApproxOrientationTop.Y);
            if (ApproxOrientationTopProj * topAxis < 0) ApproxOrientationTop = -ApproxOrientationTop;
            mx = 0; //botEllipse.Center.X;
            my = 0;// botEllipse.Center.Y;
            /*MessageBox.Show(String.Format("Number of points of bottom feature : {0}", botPnts.Length));*/

            foreach (Point pnt in botPnts)
            {
                mx += pnt.X;
                my += pnt.Y;
            }
            mx /= topPnts.Length;
            my /= topPnts.Length;

            Cxx = 0.0;
            Cyy = 0.0;
            Cxy = 0.0;

            foreach (Point pnt in botPnts)
            {
                Cxx += Math.Pow(pnt.X - mx, 2.0);
                Cyy += Math.Pow(pnt.Y - my, 2.0);
                Cxy += (pnt.X - mx) * (pnt.Y - my);
            }
            Cxx /= botPnts.Length;
            Cyy /= botPnts.Length;
            Cxy /= botPnts.Length;
            trC = Cxx + Cyy;
            detC = Cxx * Cyy - Cxy * Cxy;
            l1 = 0.5 * trC + Math.Sqrt(0.25 * trC * trC - detC);
            l2 = 0.5 * trC - Math.Sqrt(0.25 * trC * trC - detC);
            //MessageBox.Show(String.Format("({0}, {1}), ({2}, {3})", 2*botEllipse.XRadius, 2*botEllipse.YRadius, 2 * Math.Sqrt(l1), 2 * Math.Sqrt(l2)));
            a1 = 2*Math.Sqrt(l1);
            a2 = 2*Math.Sqrt(l2);
            if (a1 < a2)
            {
                double temp = a1;
                a1 = a2;
                a2 = temp;
            }
            Vector3D ApproxOrientationBot = new Vector3D(Math.Sqrt(a1 * a1 - 4*Cxx) / a1, Math.Sqrt(a1 * a1 - 4*Cyy) / a1, a2 / a1);
            Vector ApproxOrientationBotProj = new Vector(ApproxOrientationBot.X, ApproxOrientationBot.Y);
            if (ApproxOrientationBotProj * botAxis < 0) ApproxOrientationBot = -ApproxOrientationBot;
            //MessageBox.Show(String.Format("Top Vector:{0}, {1}", botAxis.X, botAxis.Y));
            //MessageBox.Show(String.Format("Bottom Vector Estimation :{0}, {1}, {2}, {3}", ApproxOrientationBot.X, ApproxOrientationBot.Y, ApproxOrientationBot.Z, ApproxOrientationBot.Length));
            Vector3D ApproxOrientation = 0.5 * (ApproxOrientationTop.Normalized() + ApproxOrientationBot.Normalized());
            //return ApproxOrientation;
            /*if (Reverse)
            {
                MessageBox.Show("Reverse!!!");
                return -ApproxOrientationBot.Normalized();
            }*/
            //else
            return ApproxOrientationBot.Normalized();
        }

        private Vector3D GetOrientation(
            EllipseParams botEllipse,
            Point[] spineApproximation,
            ref bool Reverse)
        {

            int nspine = spineApproximation.Length;

            Vector vnormal = new Vector(botEllipse.Center.X - spineApproximation[0].X, botEllipse.Center.Y + spineApproximation[0].Y);
            Vector vreverse = new Vector(botEllipse.Center.X - spineApproximation[spineApproximation.Length - 1].X, botEllipse.Center.Y + spineApproximation[spineApproximation.Length - 1].Y);

            Reverse = false;
            if (vnormal.Length > vreverse.Length) Reverse = true;
            Vector botAxis = new Vector();
            if (!Reverse)
            {
                botAxis = new Vector(spineApproximation[1].X - spineApproximation[0].X, spineApproximation[0].Y - spineApproximation[1].Y);
            }
            else
            {
                botAxis = new Vector(spineApproximation[nspine - 2].X - spineApproximation[nspine - 1].X, spineApproximation[nspine - 1].Y - spineApproximation[nspine - 2].Y);
            }

            
            var botCircleBasis = EllipseHelper.CircleOrientation(botEllipse);
            var botOrientation = GetOrientation(botCircleBasis);

            Vector ApproxOrientationBotProj = new Vector(botOrientation.X, botOrientation.Y);
            if (ApproxOrientationBotProj * botAxis < 0) botOrientation = -botOrientation;

            //var topPerimeter = EllipseHelper.ApproxPerimeter(topEllipse.XRadius, topEllipse.YRadius);
            //var botPerimeter = EllipseHelper.ApproxPerimeter(botEllipse.XRadius, botEllipse.YRadius);

            //return result;
            //if (topPerimeter > botPerimeter)
            //    return topOrientation;
            //else
            return botOrientation;
        }

        private Vector3D GetOrientation(Tuple<Vector3D, Vector3D> circleBasis)
        {
            var normal1 = Vector3D.CrossProduct(circleBasis.Item1, circleBasis.Item2);
            normal1.Normalize();
            //var normal1 = circleBasis.Item1;
            //normal1.Normalize();

            /*var normal2 = new Vector3D(normal1.X, normal1.Y, -normal1.Z);

            var normal1Difference = (normal1 - axisApproximation).LengthSquared;
            var normal2Difference = (normal2 - axisApproximation).LengthSquared;*/

            //if (normal1Difference < normal2Difference)
            return normal1;
            //else
            //    return normal2;
        }
    
    }
}
