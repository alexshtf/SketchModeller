using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using Petzold.Media3D;
using Utils;
using System.Diagnostics.Contracts;

namespace SimpleCurveEdit
{
    public class SnapTool : ITool
    {
        private bool isActive;
        private readonly Polyline snapStroke;
        private readonly Viewport3D viewport;
        private readonly ModelVisual3D curvesRoot;

        public SnapTool(Polyline snapStroke, Viewport3D viewport, ModelVisual3D curvesRoot)
        {
            this.snapStroke = snapStroke;
            this.viewport = viewport;
            this.curvesRoot = curvesRoot;
        }
         
        public void MouseDown(Point position)
        {
            isActive = true;
            snapStroke.Points = new PointCollection();
            snapStroke.Points.Add(position);
        }

        public void MouseMove(Point position)
        {
            if (isActive)
                snapStroke.Points.Add(position);
        }

        public void MouseUp(Point position)
        {
            if (isActive)
            {
                isActive = false;

                // get an array of all existing 3D curves
                var allCurves = curvesRoot.Children.OfType<ICurve>().ToArray();

                // get the points of the snap stroke and reset it
                var snapPoints = snapStroke.Points;
                snapStroke.Points = null;

                // project all curves to 2D
                var to2dTransform = curvesRoot.TransformToAncestor(viewport);
                var transformedCurves =
                    (from curve in allCurves
                     let points = curve.Points
                     select (from point in points
                             select to2dTransform.Transform(point)
                             ).ToArray()
                    ).ToArray();

                // choose the "most appropriate" curve to modify
                var chosenCurveIndex = ChooseCurveIndex(transformedCurves, snapPoints);
                var chosenCurve = allCurves[chosenCurveIndex];
                var chosenTransformed = transformedCurves[chosenCurveIndex];

                // modify the 3D curve such that part of its projection will coincide with 
                // the snap stroke.
                ModifyCurve(chosenCurve, chosenTransformed, snapPoints);
            }
        }

        /// <summary>
        /// Choosest the 'closest' curve from a set of curves, to the specified curve.
        /// </summary>
        /// <param name="transformedCurves">An array of curves to choose from (each element is an array of points)</param>
        /// <param name="snapPoints">The curve to measure distance from</param>
        /// <returns>The index of the curve in <c>transformedCurves</c></returns>
        private int ChooseCurveIndex(Point[][] transformedCurves, PointCollection snapPoints)
        {
            // will hold the amount of points in snapPoints that are closest
            // to each curve in transformedCurve. We will vote for a transformed curve for every
            // point of snapPoints that was closest to it.
            var votingArray = new int[transformedCurves.Length];

            // perform a vote for each segment in snapPoints
            foreach (var point in snapPoints)
            {
                // get closest polyline
                var closestCurveIndex =
                    transformedCurves
                    .ZipIndex()
                    .Minimizer(transformedCurve => PointDistanceFromCurve(point, transformedCurve.Value))
                    .Index;

                votingArray[closestCurveIndex]++;
            }

            // get the index of the maximum element in the voting array
            return votingArray.ZipIndex().Minimizer(x => x.Value).Index;
        }


        private double PointDistanceFromCurve(Point point, Point[] transformedCurve)
        {
            return point.ProjectionOnCurve(transformedCurve).Item2;
        }

        private void ModifyCurve(ICurve chosenCurve, Point[] transformedCurve, PointCollection snapPoints)
        {
            Contract.Requires(chosenCurve.Points.Count == transformedCurve.Length);
            Contract.Requires(chosenCurve != null);
            Contract.Requires(transformedCurve != null);
            Contract.Requires(snapPoints != null);

            var oldPnts = chosenCurve.Points.ToArray();

            // gather data from the projections on the curve
            var projectionData =
                from point in snapPoints
                let proj = point.ProjectionOnCurve(transformedCurve)
                select new { Distance = proj.Item2, Index = proj.Item3 };

            // find minimum and maximum segment index from the projections
            var minIndex = projectionData.Min(x => x.Index);
            var maxIndex = projectionData.Max(x => x.Index);

            // find average distance of snapPoints from the curve
            var avgDistance = projectionData.Average(x => x.Distance);

            // create a new curve made of optimization points
            var before =
                (from point in oldPnts.Take(minIndex + 1)
                 select new OptimizationPoint { Original = point }).ToArray();
            var after =
                (from point in oldPnts.Skip(maxIndex)
                 select new OptimizationPoint { Original = point }).ToArray();
            var middle =
                (from point in snapPoints
                 select new OptimizationPoint { ProjConstraint = point }).ToArray();

            var projTransform = new VisualInfo { ModelVisual3D = curvesRoot }.TotalTransform;
            Optimize(before, after, middle, avgDistance);

            // replace with the new curve
            chosenCurve.Points = new Point3DCollection(
                from optimizationPoint in before.Concat(middle).Concat(after)
                select optimizationPoint.New);
        }

        private void Optimize(OptimizationPoint[] before, OptimizationPoint[] after, OptimizationPoint[] middle, double avgDistance)
        {
            // get the total projection transform from 3D to 2D
            var projTransform = new VisualInfo { ModelVisual3D = curvesRoot }.TotalTransform;

            var lineOptimizer = new SnapOptimizer(before, middle, after, projTransform);
            lineOptimizer.Solve();
        }

        private Point3D Get3DPoint(Point point)
        {
            LineRange lineRange;
            if (ViewportInfo.Point2DtoPoint3D(viewport, point, out lineRange))
                return lineRange.PointFromZ(0); // the intersection of the ray with the plane Z=0
            else
                throw new InvalidOperationException("Cannot un-project the specified point. The point is invalid.");
        }
    }

    public class OptimizationPoint
    {
        public Point3D Original { get; set; }
        public Point3D New { get; set; }
        public Point ProjConstraint { get; set; }
    }
}
