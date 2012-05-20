using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using System.Windows;
using System.Diagnostics;
using System.Collections;
using SketchModeller.Utilities;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    public struct coincident_points
    {
        public coincident_points(Point P)
        {
            idxs = new List<int>();
            CP = P;
        }
        public List<int> idxs;
        public Point CP;
    }

    public static class NewPrimitiveExtensions
    {

        public static void SetColorCodingToSketch(this NewPrimitive primitive)
        {
            foreach (var pair in primitive.AllCurves.ZipIndex())
            {
                var curve = pair.Value;
                var index = pair.Index;

                if (curve.AssignedTo != null)
                    curve.AssignedTo.ColorCodingIndex = index;
            }
        }

        public static void CheckSilhoueteCurveWith1Feature(this NewPrimitive primitive)
        {
            int N = 2;
            Point[][] EndPoints = new Point[N][];
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            if (primitive.SilhouetteCurves.Length > 0 && primitive.SilhouetteCurves[1].AssignedTo != null)
            {
                EndPoints[0][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
                EndPoints[0][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
            }
            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[0].AssignedTo != null)
            {
                var FeatureCurve = primitive.FeatureCurves[0].AssignedTo;
                EndPoints[1][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                EndPoints[1][1] = new Point(10e10, 10e10);
            }
            else
            {
                if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[1].AssignedTo != null)
                {
                    var FeatureCurve = primitive.FeatureCurves[1].AssignedTo;
                    EndPoints[1][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                    EndPoints[1][1] = new Point(10e10, 10e10);
                }
            }
            List<int>[] ConnectedComponents = new List<int>[N];
            for (int count = 0; count < N; count++) ConnectedComponents[count] = new List<int>();
            for (int count = 0; count < N; count++) ConnectedComponents[count].Add(count);
            for (int pivot = 0; pivot < N; pivot++)
            {
                for (int count = 0; count < N; count++)
                {
                    if (pivot != count)
                    {
                        if (PointCompare(EndPoints[pivot], EndPoints[count]))
                        {
                            Debug.WriteLine("Indexes : " + pivot + " " + count);
                            foreach (int c in ConnectedComponents[count])
                                if (!(ConnectedComponents[pivot].Contains(c)))
                                    ConnectedComponents[pivot].Add(c);
                        }
                    }
                }
            }
            int Max = ConnectedComponents[0].Count;
            int idx = 0;
            for (int counter = 1; counter < N; counter++)
            {
                if (ConnectedComponents[counter].Count > Max)
                {
                    Max = ConnectedComponents[counter].Count;
                    idx = counter;
                }
            }
            Debug.WriteLine("1 Feature:Max=" + Max);
            if (Max < 2)
            {
                primitive.SilhouetteCurves[1].AssignedTo.isdeselected = true;
                primitive.SilhouetteCurves[1].AssignedTo = null;
            }
            else
            {
                primitive.SilhouetteCurves[1].isDeselected = true;
            }
        }

        public static void CheckSilhoueteCurveWith2Features(this NewPrimitive primitive)
        {
            int N = 3;
            Point[][] EndPoints = new Point[N][];
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            if (primitive.SilhouetteCurves.Length > 0 && primitive.SilhouetteCurves[1].AssignedTo != null)
            {
                EndPoints[0][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
                EndPoints[0][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
            }
            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[0].AssignedTo != null)
            {
                var FeatureCurve = primitive.FeatureCurves[0].AssignedTo;
                EndPoints[1][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                EndPoints[1][1] = new Point(10e5, 10e5);
            }
            if (primitive.FeatureCurves.Length > 1 && primitive.FeatureCurves[1].AssignedTo != null)
            {
                var FeatureCurve = primitive.FeatureCurves[1].AssignedTo;
                EndPoints[2][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                EndPoints[2][1] = new Point(10e10, 10e10);
            }
            List<int>[] ConnectedComponents = new List<int>[N];
            for (int count = 0; count < N; count++) ConnectedComponents[count] = new List<int>();
            for (int count = 0; count < N; count++) ConnectedComponents[count].Add(count);
            for (int pivot = 0; pivot < N; pivot++)
            {
                for (int count = 0; count < N; count++)
                {
                    if (pivot != count)
                    {
                        if (PointCompare(EndPoints[pivot], EndPoints[count]))
                        {
                            Debug.WriteLine("Indexes : " + pivot + " " + count);
                            foreach (int c in ConnectedComponents[count])
                                if (!(ConnectedComponents[pivot].Contains(c)))
                                    ConnectedComponents[pivot].Add(c);
                        }
                    }
                }
            }
            int Max = ConnectedComponents[0].Count;
            int idx = 0;
            for (int counter = 1; counter < N; counter++)
            {
                if (ConnectedComponents[counter].Count > Max)
                {
                    Max = ConnectedComponents[counter].Count;
                    idx = counter;
                }
            }
            /*bool[] bitRaised = new bool[N];
            foreach (int c in ConnectedComponents[idx]) bitRaised[c] = true;
            Max = bitRaised.Select(c => c == true).ToArray().Length;*/
            Debug.WriteLine("2 Features:Max=" + Max);
            if (Max < 3)
            {
                primitive.SilhouetteCurves[1].AssignedTo.isdeselected = true;
                primitive.SilhouetteCurves[1].AssignedTo = null;
            }
            else
            {
                primitive.SilhouetteCurves[1].isDeselected = true;
            }
        }

        public static void ValidateFeatureCurves(this NewPrimitive primitive)
        {
            int N = 2;
            Point[][] EndPoints = new Point[N][];
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            bool[] bitRaised = new bool[N];
            int SilhouettesCount = 0;
            int i = 0;
            if (primitive.SilhouetteCurves.Length > 0 && primitive.SilhouetteCurves[0].AssignedTo != null)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[0].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[0].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }
            if (primitive.SilhouetteCurves.Length > 1 && primitive.SilhouetteCurves[1].AssignedTo != null)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }

            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[0].AssignedTo != null)
            {
                var FeatureCurve1 = primitive.FeatureCurves[0].AssignedTo;
                Point[] EndFeaturePoints = new Point[2];
                EndFeaturePoints[0] = FindClosestPoint(FeatureCurve1, EndPoints[0]);
                EndFeaturePoints[1] = FindClosestPoint(FeatureCurve1, EndPoints[1]);
                var Ellipse = EllipseFitter.Fit(FeatureCurve1.Points);
                double maxperimeter = Ellipse.XRadius > Ellipse.YRadius ? 2 * Ellipse.XRadius : 2 * Ellipse.YRadius;
                Vector v = new Vector(EndFeaturePoints[0].X - EndFeaturePoints[1].X, EndFeaturePoints[0].Y - EndFeaturePoints[1].Y);
                if (Math.Min(maxperimeter, v.Length) / Math.Max(maxperimeter, v.Length) < 0.65)
                    primitive.FeatureCurves[0].AssignedTo = null;
            }
            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[1].AssignedTo != null)
            {
                var FeatureCurve1 = primitive.FeatureCurves[1].AssignedTo;
                Point[] EndFeaturePoints = new Point[2];
                EndFeaturePoints[0] = FindClosestPoint(FeatureCurve1, EndPoints[0]);
                EndFeaturePoints[1] = FindClosestPoint(FeatureCurve1, EndPoints[1]);
                var Ellipse = EllipseFitter.Fit(FeatureCurve1.Points);
                double maxperimeter = Ellipse.XRadius > Ellipse.YRadius ? 2 * Ellipse.XRadius : 2 * Ellipse.YRadius;
                Vector v = new Vector(EndFeaturePoints[0].X - EndFeaturePoints[1].X, EndFeaturePoints[0].Y - EndFeaturePoints[1].Y);
                if (Math.Min(maxperimeter, v.Length) / Math.Max(maxperimeter, v.Length) < 0.65)
                    primitive.FeatureCurves[1].AssignedTo = null;
            }
        }

        public static void CheckCubicCorner(this NewCuboid cuboid, int i)
        {
            var features = cuboid.ArrayOfCorners[i];
            int N = 3;
            Point[][] endPoints = new Point[N][];
            for (int count = 0; count < N; count++)
                endPoints[count] = new Point[2];
            endPoints[0][0] = features[0].AssignedTo.Points[0];
            endPoints[0][1] = features[0].AssignedTo.Points.Last();
            endPoints[1][0] = features[1].AssignedTo.Points[0];
            endPoints[1][1] = features[1].AssignedTo.Points.Last();
            endPoints[2][0] = features[2].AssignedTo.Points[0];
            endPoints[2][1] = features[2].AssignedTo.Points.Last();
            coincident_points[] connectedComponents = new coincident_points[N];
            for (int count = 0; count < N; count++)
            {
                connectedComponents[count].idxs = new List<int>();
                connectedComponents[count].idxs.Add(count);
                connectedComponents[count].CP = new Point(10e10, 10e10);
            }
            int stride = 0;
            for (int pivot = stride; pivot < N; pivot += stride)
            {
                for (int count = pivot + 1; count < N; count++)
                {
                    if (pivot != count)
                    {
                        Point? maybeCp = PointCompareReturn(endPoints[pivot], endPoints[count]);
                        if (maybeCp != null)
                        {
                            var cp = maybeCp.Value;
                            foreach (int c in connectedComponents[count].idxs)
                                if (!(connectedComponents[pivot].idxs.Contains(c)))
                                    if (connectedComponents[pivot].idxs.Count < 2)
                                    {
                                        connectedComponents[pivot].idxs.Add(c);
                                        connectedComponents[pivot].CP = cp;
                                    }
                                    else if (TwoPointCompare(cp, connectedComponents[pivot].CP))
                                    {
                                        Debug.WriteLine("({0},{1}), ({2},{3})", cp.X, cp.Y, connectedComponents[pivot].CP.X, connectedComponents[pivot].CP.Y);
                                        connectedComponents[pivot].idxs.Add(c);
                                    }
                        }
                    }
                    stride++;
                }
            }
            int max = connectedComponents[0].idxs.Count;
            int idx = 0;
            for (int counter = 1; counter < N; counter++)
            {
                if (connectedComponents[counter].idxs.Count > max)
                {
                    max = connectedComponents[counter].idxs.Count;
                    idx = counter;
                }
            }
            bool[] bitRaised = new bool[N];
            foreach (int c in connectedComponents[idx].idxs) bitRaised[c] = true;
            for (int counter = 0; counter < N; counter++)
            {
                if (!bitRaised[counter])
                {
                    if (features[counter].AssignedTo != null)
                        features[counter].AssignedTo.isdeselected = true;
                    features[counter].AssignedTo = null;

                }
                else
                {
                    features[counter].AssignedTo.isdeselected = true;
                    features[counter].isDeselected = true;
                }
            }
        }

        //We assume that there is one Feature Curve
        public static void CheckFeatureCurves(this NewPrimitive primitive)
        {
            int N = 3;
            Point[][] EndPoints = new Point[N][];
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            if (primitive.SilhouetteCurves.Length > 0 && primitive.SilhouetteCurves[0].AssignedTo != null)
            {
                EndPoints[0][0] = primitive.SilhouetteCurves[0].AssignedTo.Points[0];
                EndPoints[0][1] = primitive.SilhouetteCurves[0].AssignedTo.Points.Last();
            }
            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[0].AssignedTo != null)
            {
                var FeatureCurve = primitive.FeatureCurves[0].AssignedTo;
                EndPoints[1][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                EndPoints[1][1] = new Point(10e10, 10e10);
            }
            else
            {
                EndPoints[1][0] = new Point(10e5, 10e5);
                EndPoints[1][1] = new Point(-10e5, -10e5);
            }
            if (primitive.FeatureCurves.Length > 1 && primitive.FeatureCurves[1].AssignedTo != null)
            {
                var FeatureCurve = primitive.FeatureCurves[1].AssignedTo;
                EndPoints[2][0] = FindClosestPoint(FeatureCurve, EndPoints[0]);
                EndPoints[2][1] = new Point(-10e10, -10e10);
            }
            else
            {
                EndPoints[2][0] = new Point(10e2, 10e2);
                EndPoints[2][1] = new Point(-10e2, -10e2);
            }

            List<int>[] ConnectedComponents = new List<int>[N];
            for (int count = 0; count < N; count++) ConnectedComponents[count] = new List<int>();
            for (int count = 0; count < N; count++) ConnectedComponents[count].Add(count);
            for (int pivot = 0; pivot < N; pivot++)
            {
                for (int count = 0; count < N; count++)
                {
                    if (pivot != count)
                    {
                        if (PointCompare(EndPoints[pivot], EndPoints[count]))
                        {
                            foreach (int c in ConnectedComponents[count])
                                if (!(ConnectedComponents[pivot].Contains(c)))
                                    ConnectedComponents[pivot].Add(c);
                        }
                    }
                }
            }
            int Max = ConnectedComponents[0].Count;
            int idx = 0;
            for (int counter = 1; counter < N; counter++)
            {
                if (ConnectedComponents[counter].Count > Max)
                {
                    Max = ConnectedComponents[counter].Count;
                    idx = counter;
                }
            }
            bool[] bitRaised = new bool[N];
            foreach (int c in ConnectedComponents[idx]) bitRaised[c] = true;
            for (int counter = 0; counter < N; counter++)
            {
                if (!bitRaised[counter])
                {
                    if (counter > 0)
                    {
                        if (primitive.FeatureCurves[counter - 1].AssignedTo != null)
                            primitive.FeatureCurves[counter - 1].AssignedTo.isdeselected = true;
                        primitive.FeatureCurves[counter - 1].AssignedTo = null;
                    }
                }
                else if (counter > 0)
                {
                    primitive.FeatureCurves[counter - 1].AssignedTo.isdeselected = true;
                    primitive.FeatureCurves[counter - 1].isDeselected = true;
                }
            }
        }

        public static void GetLargestComponet(this NewPrimitive primitive, bool FirstSilhouette = false)
        {
            var ActiveCurves = primitive.AllCurves
                .Select(c => c)
                .Where(c => c.AssignedTo != null)
                .ToArray();
            int N = ActiveCurves.Length;
            Point[][] EndPoints = new Point[N][];
            //Debug.WriteLine("Number of Active Curves="+N);
            //Debug.WriteLine("Feature Curves Number : " + primitive.FeatureCurves.Length);
            //Debug.WriteLine("Silhouette Curves Number : " + primitive.SilhouetteCurves.Length);
            bool[] ActiveFeatureCurve = { true, true };
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            bool[] bitRaised = new bool[N];
            int SilhouettesCount = 0;
            int i = 0;
            if (primitive.SilhouetteCurves.Length > 0 && primitive.SilhouetteCurves[0].AssignedTo != null)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[0].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[0].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }
            if (primitive.SilhouetteCurves.Length > 1 && primitive.SilhouetteCurves[1].AssignedTo != null)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }
            //PointsSequence FeatureCurve1 = new PointsSequence(); 
            if (primitive.FeatureCurves.Length > 0 && primitive.FeatureCurves[0].AssignedTo != null)
            {
                var FeatureCurve1 = primitive.FeatureCurves[0].AssignedTo;
                if (SilhouettesCount > 0) EndPoints[i][0] = FindClosestPoint(FeatureCurve1, EndPoints[0]);
                if (SilhouettesCount > 1) EndPoints[i][1] = FindClosestPoint(FeatureCurve1, EndPoints[1]);
                else if (SilhouettesCount > 0) EndPoints[i][1] = EndPoints[i][0];
                if (SilhouettesCount > 0)
                {
                    var Ellipse = EllipseFitter.Fit(FeatureCurve1.Points);
                    double maxperimeter = Ellipse.XRadius > Ellipse.YRadius ? 2 * Ellipse.XRadius : 2 * Ellipse.YRadius;
                    Vector v = new Vector(EndPoints[i][0].X - EndPoints[i][1].X, EndPoints[i][0].Y - EndPoints[i][1].Y);
                    if (Math.Min(maxperimeter, v.Length) / Math.Max(maxperimeter, v.Length) < 0.7) ActiveFeatureCurve[0] = false;
                    if (!ActiveFeatureCurve[0])
                    {
                        EndPoints[i][0].X = 10e11;
                        EndPoints[i][1].X = 10e11;
                        EndPoints[i][0].Y = 10e11;
                        EndPoints[i][1].Y = 10e11;
                    }
                    i++;
                }
            }
            Debug.WriteLine("Number of Silhouettes:" + SilhouettesCount);
            if (primitive.FeatureCurves.Length > 1 && primitive.FeatureCurves[1].AssignedTo != null)
            {
                var FeatureCurve2 = primitive.FeatureCurves[1].AssignedTo;
                if (SilhouettesCount > 0) EndPoints[i][0] = FindClosestPoint(FeatureCurve2, EndPoints[0]);
                if (SilhouettesCount > 1) EndPoints[i][1] = FindClosestPoint(FeatureCurve2, EndPoints[1]);
                else if (SilhouettesCount > 0) EndPoints[i][1] = EndPoints[i][0];
                if (SilhouettesCount > 0)
                {
                    var Ellipse = EllipseFitter.Fit(FeatureCurve2.Points);
                    double maxperimeter = Ellipse.XRadius > Ellipse.YRadius ? 2 * Ellipse.XRadius : 2 * Ellipse.YRadius;
                    Vector v = new Vector(EndPoints[i][0].X - EndPoints[i][1].X, EndPoints[i][0].Y - EndPoints[i][1].Y);
                    if (Math.Min(maxperimeter, v.Length) / Math.Max(maxperimeter, v.Length) < 0.7) ActiveFeatureCurve[1] = false;
                    if (!ActiveFeatureCurve[1])
                    {
                        EndPoints[i][0].X = 10e10;
                        EndPoints[i][1].X = 10e10;
                        EndPoints[i][0].Y = 10e10;
                        EndPoints[i][1].Y = 10e10;
                    }
                    i++;
                }
            }

            List<int>[] ConnectedComponents = new List<int>[N];
            for (int count = 0; count < N; count++) ConnectedComponents[count] = new List<int>();
            for (int count = 0; count < N; count++) ConnectedComponents[count].Add(count);
            //int offset = 1;
            for (int pivot = 0; pivot < N; pivot++)
            {
                for (int count = 0; count < N; count++)
                {
                    if (pivot != count)
                    {
                        if (PointCompare(EndPoints[pivot], EndPoints[count]))
                        {
                            foreach (int c in ConnectedComponents[count])
                                if (!(ConnectedComponents[pivot].Contains(c)))
                                    ConnectedComponents[pivot].Add(c);
                        }
                    }
                }
            }
            int Max = ConnectedComponents[0].Count;
            int idx = 0;
            for (int counter = 1; counter < N; counter++)
            {
                if (ConnectedComponents[counter].Count > Max)
                {
                    Max = ConnectedComponents[counter].Count;
                    idx = counter;
                }
            }


            //Debug.WriteLine("Curves in Component = " + Max);
            foreach (int c in ConnectedComponents[idx]) bitRaised[c] = true;
            for (int counter = 0; counter < N; counter++)
                if (!bitRaised[counter])
                {
                    /*ActiveCurves[counter].AssignedTo.isdeselected = true;
                    ActiveCurves[counter].AssignedTo = null;*/
                    if (counter == 1)
                    {
                        primitive.SilhouetteCurves[counter].AssignedTo.isdeselected = true;
                        primitive.SilhouetteCurves[counter].AssignedTo = null;
                    }
                    else if (counter == 0 && FirstSilhouette)
                    {
                        primitive.SilhouetteCurves[counter].AssignedTo.isdeselected = true;
                        //primitive.SilhouetteCurves[counter].AssignedTo = null;
                    }
                    else if (counter > 1)
                    {
                        primitive.FeatureCurves[counter - 2].AssignedTo.isdeselected = true;
                        primitive.FeatureCurves[counter - 2].AssignedTo = null;
                    }
                }
                else
                {

                    if (counter > 1)
                    {
                        if (!ActiveFeatureCurve[counter - 2])
                        {
                            primitive.FeatureCurves[counter - 2].AssignedTo.isdeselected = true;
                            primitive.FeatureCurves[counter - 2].AssignedTo = null;
                        }
                    }
                }

        }

        public static Point FindClosestPoint(PointsSequence FeatureCurve, Point[] EndPoints)
        {
            double Min = 10e10;
            Point Pmin = new Point();
            foreach (Point pnt in FeatureCurve.Points)
            {
                Vector v1 = new Vector(pnt.X - EndPoints[0].X, pnt.Y - EndPoints[0].Y);
                Vector v2 = new Vector(pnt.X - EndPoints[1].X, pnt.Y - EndPoints[1].Y);
                double vmin = v1.Length < v2.Length ? v1.Length : v2.Length;
                if (Min > vmin)
                {
                    Pmin = pnt;
                    Min = vmin;
                }
            }
            return Pmin;
        }

        public static bool PointCompare(Point[] P1, Point[] P2)
        {
            double[] distances = new double[4];
            Vector v1 = new Vector(P1[0].X - P2[0].X, P1[0].Y - P2[0].Y);
            Vector v2 = new Vector(P1[0].X - P2[1].X, P1[0].Y - P2[1].Y);
            Vector v3 = new Vector(P1[1].X - P2[0].X, P1[1].Y - P2[0].Y);
            Vector v4 = new Vector(P1[1].X - P2[1].X, P1[1].Y - P2[1].Y);
            distances[0] = v1.Length;
            distances[1] = v2.Length;
            distances[2] = v3.Length;
            distances[3] = v4.Length;
            double min = distances.Min();
            Debug.WriteLine("Distance:" + min);
            if (min < 0.05) return true;
            else return false;
        }

        public static bool TwoPointCompare(Point P1, Point P2)
        {
            Vector v1 = new Vector(P1.X - P2.X, P1.Y - P2.Y);
            if (v1.Length < 0.05) return true;
            else return false;
        }

        public static bool Two3DPointCompare(Point3D P1, Point3D P2)
        {
            Vector3D v1 = new Vector3D(P1.X - P2.X, P1.Y - P2.Y, P1.Z - P2.Z);
            if (v1.Length < 0.05) return true;
            else return false;
        }

        /// <summary>
        /// Returns the closest point on p1 to any point on p2 if the closest distance is below
        /// the specified threshold, and null otherwise.
        /// </summary>
        /// <param name="threshold">The comparison threshold for the closest distance</param>
        /// <returns></returns>
        public static Point? PointCompareReturn(Point[] P1, Point[] P2, double threshold = 0.15)
        {
            var p1WithDistances = from pointIdx in P1.ZipIndex()
                                  let pointOnP1 = pointIdx.Value
                                  let index = pointIdx.Index
                                  let minDistance = P2.Min(pointOnP2 => (pointOnP1 - pointOnP2).Length)
                                  where minDistance < threshold
                                  select new { Point = pointOnP1, MinDistance = minDistance };

            if (p1WithDistances.Any())
                return p1WithDistances.Minimizer(x => x.MinDistance).Point;
            else
                return null;
        }

        public static void ClearColorCodingFromSketch(this NewPrimitive primitive)
        {
            var assignedQuery =
                from curves in primitive.AllCurves
                where curves.AssignedTo != null
                select curves.AssignedTo;

            foreach (var sketchCurve in assignedQuery)
                sketchCurve.ColorCodingIndex = PointsSequence.INVALID_COLOR_CODING;
        }

        public static Point FindEndPoint(PointsSequence ps, Point StartPoint)
        {
            Point P1 = ps.Points[0];
            Point P2 = ps.Points.Last();
            return TwoPointCompare(P1, StartPoint) ? P2 : P1;
        }

        public static Point3D FindEnd3DPoint(Point3D[] P, Point3D StartPoint)
        {
            Point3D P1 = P[0];
            Point3D P2 = P[1];
            return Two3DPointCompare(P1, StartPoint) ? P2 : P1;
        }

        public static Point FindCoincidentPoint(PrimitiveCurve[] curves, bool Sketch = true)
        {
            Point[][] EndPoints = new Point[2][];
            for (int count = 0; count < 2; count++)
                EndPoints[count] = new Point[2];

            EndPoints[0][0] = Sketch ? curves[0].AssignedTo.Points[0] : curves[0].Points[0];
            EndPoints[0][1] = Sketch ? curves[0].AssignedTo.Points.Last() : curves[0].Points[1];
            EndPoints[1][0] = Sketch ? curves[1].AssignedTo.Points[0] : curves[1].Points[0];
            EndPoints[1][1] = Sketch ? curves[1].AssignedTo.Points.Last() : curves[1].Points[1];

            Point? CP = PointCompareReturn(EndPoints[0], EndPoints[1]);
            if (CP != null)
                return CP.Value;
            else
                return new Point(double.MaxValue, double.MaxValue);
        }

        public static Point3D FindCoincident3DPoint(PrimitiveCurve[] curves)
        {
            Point3D[][] EndPoints = new Point3D[2][];
            for (int count = 0; count < 2; count++)
                EndPoints[count] = new Point3D[2];
            EnhancedPrimitiveCurve epc = (EnhancedPrimitiveCurve)curves[0];
            EndPoints[0][0] = epc.Points3D[0];
            EndPoints[0][1] = epc.Points3D[1];
            epc = (EnhancedPrimitiveCurve)curves[1];
            EndPoints[1][0] = epc.Points3D[0];
            EndPoints[1][1] = epc.Points3D[1];
            Point3D CP = Point3DCompareReturn(EndPoints[0], EndPoints[1]);
            return CP;
        }

        public static Point3D Point3DCompareReturn(Point3D[] P1, Point3D[] P2)
        {
            double[] distances = new double[4];
            Vector3D v1 = new Vector3D(P1[0].X - P2[0].X, P1[0].Y - P2[0].Y, P1[0].Z - P2[0].Z);
            Vector3D v2 = new Vector3D(P1[0].X - P2[1].X, P1[0].Y - P2[1].Y, P1[0].Z - P2[1].Z);
            Vector3D v3 = new Vector3D(P1[1].X - P2[0].X, P1[1].Y - P2[0].Y, P1[1].Z - P2[0].Z);
            Vector3D v4 = new Vector3D(P1[1].X - P2[1].X, P1[1].Y - P2[1].Y, P1[1].Z - P2[1].Z);
            distances[0] = v1.Length;
            distances[1] = v2.Length;
            distances[2] = v3.Length;
            distances[3] = v4.Length;
            double min = distances[0];
            int idx = 0;
            for (int i = 1; i < 4; i++)
                if (min > distances[i])
                {
                    min = distances[i];
                    idx = i;
                }
            //Debug.WriteLine("Distance:" + min);
            if (min < 0.05)
                return idx < 2 ? P1[0] : P1[1];
            else return new Point3D(10e10, 10e10, 10e10);
        }

    }

}
