using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using System.Windows;
using System.Diagnostics;
using System.Collections;

namespace SketchModeller.Infrastructure.Data
{
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

        public static void GetLargestComponet(this NewPrimitive primitive)
        {
            var ActiveCurves = primitive.AllCurves
                .Select(c => c)
                .Where(c => c != null)
                .ToArray();
            int N = 4;
            //Debug.WriteLine("Number of Active Curves="+N);
            Point[][] EndPoints = new Point[N][];
            //Debug.WriteLine("Feature Curves Number : " + primitive.FeatureCurves.Length);
            //Debug.WriteLine("Silhouette Curves Number : " + primitive.SilhouetteCurves.Length);
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            bool[] bitRaised = new bool[N];
            int i = 0;
            EndPoints[0][0] = primitive.SilhouetteCurves[0].AssignedTo.Points[0];
            EndPoints[0][1] = primitive.SilhouetteCurves[0].AssignedTo.Points.Last();
            EndPoints[1][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
            EndPoints[1][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
            
            var FeatureCurve1 = primitive.FeatureCurves[0].AssignedTo;
            var FeatureCurve2 = primitive.FeatureCurves[1].AssignedTo;
            
            EndPoints[2][0] = FindClosestPoint(FeatureCurve1, EndPoints[0]);
            EndPoints[2][1] = FindClosestPoint(FeatureCurve1, EndPoints[1]);
            EndPoints[3][0] = FindClosestPoint(FeatureCurve2, EndPoints[0]);
            EndPoints[3][1] = FindClosestPoint(FeatureCurve2, EndPoints[1]);

            List<int>[] ConnectedComponents = new List<int>[N];
            for (int count = 0; count < N; count++) ConnectedComponents[count] = new List<int>();
            for (int count = 0; count < N; count++ ) ConnectedComponents[count].Add(count);
            //int offset = 1;
            for (int pivot = 0; pivot < N; pivot++)
            {
                for (int count = 0; count < N; count ++)
                {
                    if (pivot != count){
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
                    if (counter < 2)
                    {
                        primitive.SilhouetteCurves[counter].AssignedTo.isdeselected = true;
                        primitive.SilhouetteCurves[counter].AssignedTo = null;
                    }
                    else
                    {
                        primitive.FeatureCurves[counter - 2].AssignedTo.isdeselected = true;
                        primitive.FeatureCurves[counter-2].AssignedTo = null;
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
            if (min < 0.05) return true;
            else return false;
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
    }
}
