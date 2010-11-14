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
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MultiviewCurvesToCyl
{
    class PointsBasedCylinderViewModel : Base3DViewModel
    {
        private const double SKELETON_SIZE_FACTOR = 0.1;
        private const int MIN_SKELETN_SIZE = 20;

        private const double RADIUS_SLICES_FACTOR = 0.5;
        private const int MIN_SLICES = 10;

        private SkeletonPoint[] skeleton;

        private Point3D[] meshPositions;
        private Vector3D[] meshNormals;
        private int[] meshIndices;

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
        public void Initialize(bool wireframe)
        {
            Contract.Requires(IsInitialized == false, "Cannot initialize the object twice");

            IsInWireframeMode = wireframe;
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

            Contract.Assume(curves3d.Count() == 2);
            var firstCurve = curves3d.ElementAt(0).ToArray();
            var secondCurve = curves3d.ElementAt(1).ToArray();
            FlipSecondIfNecessary(firstCurve, secondCurve);

            // get skeleton size
            var maxDistanceSquared = MaxDistanceSquared(firstCurve, secondCurve);
            var skeletonSize = Math.Max(MIN_SKELETN_SIZE, (int)Math.Round(Math.Sqrt(maxDistanceSquared) * SKELETON_SIZE_FACTOR));
         
            // extract first/last wanted cylinder points
            var firstSkeletonPoint = MathUtils3D.Lerp(firstCurve.First(), secondCurve.First(), 0.5);
            var lastSkeletonPoint =  MathUtils3D.Lerp(firstCurve.Last(), secondCurve.Last(), 0.5);

            // generate and iteratively-improve mesh skeleton
            var currApproximation = CreateSkeletonApproximation(firstSkeletonPoint, lastSkeletonPoint, skeletonSize);
            for (int i = 0; i < 3; ++i)
            {
                currApproximation = ImproveApproximation(firstCurve, secondCurve, currApproximation);
                currApproximation = UniformSamplePolyline(currApproximation);
            }
            skeleton = BuildSkeletonFromPoints(firstCurve, secondCurve, currApproximation);

            // create the mesh data
            var maxRadius = skeleton.Select(x => x.Radius).Max();
            var slicesCount = Math.Max(MIN_SLICES, (int)Math.Round(maxRadius * RADIUS_SLICES_FACTOR));
            var meshData = SkeletonToMesh.SkeletonToCylinder(skeleton, slicesCount);

            // set mesh properties + notify outside world.
            Positions = meshData.Item1;
            Normals = meshData.Item2;
            TriangleIndices = meshData.Item3;
        }

        /// <summary>
        /// Mesh vertex positions
        /// </summary>
        public Point3D[] Positions
        {
            get { return meshPositions; }
            private set
            {
                meshPositions = value;
                NotifyPropertyChanged(() => Positions);
            }
        }

        /// <summary>
        /// Mesh normals
        /// </summary>
        public Vector3D[] Normals
        {
            get { return meshNormals; }
            private set
            {
                meshNormals = value;
                NotifyPropertyChanged(() => Normals);
            }
        }

        /// <summary>
        /// Mesh triangle indices
        /// </summary>
        public int[] TriangleIndices
        {
            get { return meshIndices; }
            set
            {
                meshIndices = value;
                NotifyPropertyChanged(() =>TriangleIndices);
            }
        }

        private Point3D[] UniformSamplePolyline(Point3D[] currApproximation)
        {
            var result = new Point3D[currApproximation.Length];

            // calculate arc length
            var polylineLength =
                (from seg in currApproximation.SeqPairs()
                 let p1 = seg.Item1
                 let p2 = seg.Item2
                 select (p2 - p1).Length
                ).Sum();

            // segment-length helper function (length of the i-th segment)
            Func<int, double> segmentLength = idx =>
                {
                    var p1 = currApproximation[idx + 0];
                    var p2 = currApproximation[idx + 1];
                    return (p2 - p1).Length;
                };

            // main sampling loop
            int segsCount = currApproximation.Length - 1;
            int currSeg = 0;
            double lengthOnPoly = 0;
            for (int i = 0; i < result.Length; ++i)
            {
                var targetLength = polylineLength * (double)i / (double)(result.Length - 1);
                while (currSeg < segsCount &&
                       lengthOnPoly + segmentLength(currSeg) < targetLength)
                {
                    lengthOnPoly = lengthOnPoly + segmentLength(currSeg);
                    currSeg = currSeg + 1;
                }

                var lengthOnSeg = targetLength - lengthOnPoly;
                var t = lengthOnSeg / segmentLength(currSeg);
                result[i] = MathUtils3D.Lerp(currApproximation[currSeg], currApproximation[currSeg + 1], t);
            }

            return result;
        }

        private Point3D[] ImproveApproximation(
            Point3D[] firstCurve, 
            Point3D[] secondCurve, 
            Point3D[] currentApproximation)
        {
            var result = new Point3D[currentApproximation.Length];
            var partitioner = Partitioner.Create(0, currentApproximation.Length);

            Parallel.ForEach(partitioner, (range, loopstate) =>
                {
                    for (int i = range.Item1; i < range.Item2; ++i)
                    {
                        var p1 = currentApproximation[i].ProjectionOnCurve(firstCurve).Position;
                        var p2 = currentApproximation[i].ProjectionOnCurve(secondCurve).Position;
                        result[i] = MathUtils3D.Lerp(p1, p2, 0.5);
                    }
                });

            return result;
        }

        private Point3D[] CreateSkeletonApproximation(
            Point3D firstSkeletonPoint, 
            Point3D lastSkeletonPoint, 
            int skeletonSize)
        {
            Point3D[] resultPoints = new Point3D[skeletonSize];
            for (int i = 0; i < skeletonSize; ++i)
            {
                double t = i / (double)(skeletonSize - 1);
                var pnt = MathUtils3D.Lerp(firstSkeletonPoint, lastSkeletonPoint, t);
                resultPoints[i] = pnt;
            }

            return resultPoints;
        }

        private static SkeletonPoint[] BuildSkeletonFromPoints(Point3D[] firstCurve, Point3D[] secondCurve, Point3D[] skeletonPositions)
        {
            var skeletonSize = skeletonPositions.Length;
            var result = new SkeletonPoint[skeletonSize];

            var partitioner = Partitioner.Create(0, skeletonSize);
            Parallel.ForEach(partitioner, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; ++i)
                    {
                        var pos = skeletonPositions[i];
                        result[i] = new SkeletonPoint();
                        result[i].Position = pos;
                        result[i].Radius =
                            0.5 * pos.ProjectionOnCurve(firstCurve).Distance +
                            0.5 * pos.ProjectionOnCurve(secondCurve).Distance;
                        if (i < skeletonSize - 1)
                            result[i].Normal = skeletonPositions[i + 1] - skeletonPositions[i];
                    }

                });
            result[skeletonSize - 1].Normal = result[skeletonSize - 2].Normal;

            for (int i = 0; i < skeletonSize; ++i)
                result[i].Normal = result[i].Normal.Normalized();

            return result;
        }

        private double MaxDistanceSquared(Point3D[] firstCurve, Point3D[] secondCurve)
        {
            var allPoints = firstCurve.Concat(secondCurve).ToArray();
            var maxDistance = 0.0;
            for (int i = 0; i < allPoints.Length; ++i)
            {
                for (int j = i + 1; j < allPoints.Length; ++j)
                {
                    var currDistance = (allPoints[i] - allPoints[j]).LengthSquared;
                    if (currDistance > maxDistance)
                        maxDistance = currDistance;
                }
            }

            return maxDistance;
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
    }
}
