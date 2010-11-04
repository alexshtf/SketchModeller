using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiviewCurvesToCyl.Base;
using AutoDiff;
using System.Windows.Media.Media3D;
using Utils;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Diagnostics;

namespace MultiviewCurvesToCyl
{
    class PointsBasedCylinderViewModel : Base3DViewModel
    {
        private const double SOFT_MIN_MAX_POWER = 10;
        private const int MIN_SKELETN_SIZE = 20;
        private const double SKELETON_SIZE_FACTOR = 0.1;

        private double radius;
        private double length;
        private Point3D center;
        private Vector3D orientation;
        private IHaveCameraInfo cameraInfo;
        private SkeletonPoint[] skeleton;

        public PointsBasedCylinderViewModel()
        {
            skeleton = new SkeletonPoint[] { 
                new SkeletonPoint 
                { 
                    Position = new Point3D(0,0,0), 
                    Normal = MathUtils3D.UnitX, 
                    Radius = 1.0, 
                },
                new SkeletonPoint
                {
                    Position = new Point3D(1,0,0), 
                    Normal = MathUtils3D.UnitX, 
                    Radius = 1.0,
                }};
        }

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the cylinder data
        /// </summary>
        /// <param name="radius">The cylinder radius.</param>
        /// <param name="length">The cylinder length.</param>
        /// <param name="center">The cylinder center.</param>
        /// <param name="orientation">The cylinder orientation.</param>
        /// <param name="initCameraInfo">The camera info</param>
        public void Initialize(double radius, double length, Point3D center, Vector3D orientation, IHaveCameraInfo initCameraInfo, bool wireframe)
        {
            Contract.Requires(IsInitialized == false, "Cannot initialize the object twice");
            Contract.Requires(initCameraInfo != null);

            cameraInfo = initCameraInfo;
            IsInWireframeMode = wireframe;
            this.radius = radius;
            this.length = length;
            this.center = center;
            this.orientation = orientation;
            IsInitialized = true;
        }

        
        /// <summary>
        /// Snaps the cylinder to the specified curves.
        /// </summary>
        /// <param name="curves">The curves.</param>
        public void SnapTo(IEnumerable<SketchCurve> curves)
        {
            Contract.Requires(curves != null);
            Contract.Requires(curves.Count() == 2);
            Contract.Requires(Contract.ForAll(curves, curve => curve.HasAnnotation<StartEndAnnotation>()));

            var curves3d =
                from curve in curves
                select GetCurvePoints(curve);
            curves3d = curves3d.ToArray();

            var skeletonSize = Math.Max(MIN_SKELETN_SIZE, (int)Math.Round(length * SKELETON_SIZE_FACTOR));
            var vars = ArrayUtils.Generate<Variable>(3 * skeletonSize);
            var varPoints = new TermVector3D[skeletonSize];
            for (int i = 0; i < skeletonSize; ++i)
                varPoints[i] = new TermVector3D(vars[3*i + 0], vars[3*i + 1], vars[3*i + 2]);

            Contract.Assume(curves3d.Count() == 2);
            var firstCurve = curves3d.ElementAt(0).ToArray();
            var secondCurve = curves3d.ElementAt(1).ToArray();
            FlipSecondIfNecessary(firstCurve, secondCurve);

            // we should minimize the maximal distance of a skeleton point from both curves
            var maxDistances =
                from varPoint in varPoints
                let fstDistances = from pnt in firstCurve
                                   select (varPoint - pnt).LengthSquared
                let sndDistances = from pnt in secondCurve
                                   select (varPoint - pnt).LengthSquared
                let d1 = SoftMin(fstDistances) // distance from first curve
                let d2 = SoftMin(sndDistances) // distsnce from second curve
                select SoftMax(d1, d2);
            var maxDistancesTerm = TermBuilder.Sum(maxDistances);

            // we should minimize the laplacian magnitudes so that our skeleton will be as smooth as possible
            var seqLaplacian =
                from triple in varPoints.SeqTripples()
                let p1 = triple.Item1
                let p2 = triple.Item2
                let p3 = triple.Item3
                select (p2 - 0.5 * (p1 + p3)).LengthSquared;
            var seqDistancesTerm = TermBuilder.Sum(seqLaplacian);

            // put hard constraints on the positions of the first and last points
            var hardConstraintsTerm =
                (varPoints.First() - MathUtils3D.Lerp(firstCurve.First(), secondCurve.First(), 0.5)).LengthSquared +
                (varPoints.Last() - MathUtils3D.Lerp(firstCurve.Last(), secondCurve.Last(), 0.5)).LengthSquared;

            var targetFunction = 0.1 * maxDistancesTerm + seqDistancesTerm + 1000 * hardConstraintsTerm;
            var minimum = Minimize(targetFunction, vars, new double[vars.Length]);
            var skeletonPositions = new Point3D[varPoints.Length];
            for (int i = 0; i < skeletonPositions.Length; ++i)
                skeletonPositions[i] = new Point3D(minimum[3 * i + 0], minimum[3 * i + 1], minimum[3 * i + 2]);
        }

        private static SkeletonPoint[] BuildSkeleton(Point3D[] firstCurve, Point3D[] secondCurve, Point3D[] skeletonPositions)
        {
            // TODO: Build skeleton here.
            return null;
        }

        private double[] Minimize(Term targetFunction, Variable[] vars, double[] initial)
        {
            var optimizer = new GradientDescentOptimizer(
                targetFunction, 
                vars, 
                (x, gradient, targetFunc, variables) => 
                    FastGradientOptimizer.InexactStepSize(x, gradient, targetFunc, variables, 1.0, 1.2, 0.001));
            
            var epsilon = 1E-1;

            foreach (var stepResult in optimizer.Minimize(initial))
            {
                if (Math.Abs(stepResult.CurrentTarget - stepResult.PrevTarget) < epsilon)
                    return stepResult.CurrentMinimizer;
            }

            // we should never reach here. The above look is infinite unless we return from it.
            return null;
        }


        private void FlipSecondIfNecessary(Point3D[] firstCurve, Point3D[] secondCurve)
        {
            var p1 = firstCurve.First();
            var pn = firstCurve.Last();
            var q1 = secondCurve.First();
            var qn = secondCurve.Last();

            // calc normal distances
            var d1 = (p1 - q1).LengthSquared;
            var d2 = (pn - qn).LengthSquared;

            // calc flipped distances
            var df1 = (p1 - qn).LengthSquared;
            var df2 = (q1 - pn).LengthSquared;

            if (df1 + df2 < d1 + d2)
                Array.Reverse(secondCurve);
        }

        private static IEnumerable<Point3D> GetCurvePoints(SketchCurve curve)
        {
            var startEndAnnotation = curve.GetAnnotations<StartEndAnnotation>().First();

            var minIndex = Math.Min(startEndAnnotation.StartIndex, startEndAnnotation.EndIndex);
            var maxIndex = Math.Max(startEndAnnotation.StartIndex, startEndAnnotation.EndIndex);
            var slice = new ListSlice<Point>(curve.PolylinePoints, minIndex, maxIndex + 1); // maxIndex + 1 because ListSlice is exclusive of the end index.

            return
                from pnt in slice
                select new Point3D(pnt.X, pnt.Y, 0);
        }

        #region Term building math functions

        private static Term SoftMax(Term first, Term second)
        {
            var sum = TermBuilder.Power(first, SOFT_MIN_MAX_POWER) + TermBuilder.Power(second, SOFT_MIN_MAX_POWER);
            return TermBuilder.Power(sum, 1 / SOFT_MIN_MAX_POWER);
        }

        private static Term SoftMin(IEnumerable<Term> terms)
        {
            var powers = from term in terms
                         select TermBuilder.Power(term, -SOFT_MIN_MAX_POWER);
            var result =
                TermBuilder.Power(TermBuilder.Sum(powers), -1 / SOFT_MIN_MAX_POWER);
            return result;
        }

        private static Term PointSegmentDistanceSquared(TermVector3D point, Point3D segStart, Point3D segEnd)
        {
            var segVec = segEnd - segStart;
            var recipLengthSquared = 1 / segVec.LengthSquared;

            var t = ((point - segStart) * segVec) * recipLengthSquared;
            return TermBuilder.Piecewise(
                Tuple.Create(t.LessThanEquals(0), (point - segStart).LengthSquared),    // ||point - segStart|| when t <= 0
                Tuple.Create(t.GreaterThanEquals(1), (point - segEnd).LengthSquared),   // ||point - segEnd|| when t >= 0
                Tuple.Create(Inequality.AlwaysTrue, PointLineDistanceSquared(point, segStart, segVec, recipLengthSquared))); // normal point-line distance otherwise

        }

        private static Term PointLineDistanceSquared(TermVector3D point, Point3D segStart, Vector3D segVec, double recipLengthSquared)
        {
            return TermVector3D.CrossProduct(segVec, segStart - point).LengthSquared * recipLengthSquared;
        }

        #endregion
    }
}
