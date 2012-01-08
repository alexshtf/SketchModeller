﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using System.Windows;
using System.Diagnostics;
using System.Collections;
using SketchModeller.Utilities;

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
                .Where(c => c.AssignedTo != null)
                .ToArray();
            int N = ActiveCurves.Length;
            //Debug.WriteLine("Number of Active Curves="+N);
            Point[][] EndPoints = new Point[N][];
            //Debug.WriteLine("Feature Curves Number : " + primitive.FeatureCurves.Length);
            //Debug.WriteLine("Silhouette Curves Number : " + primitive.SilhouetteCurves.Length);
            bool[] ActiveFeatureCurve = { true, true };
            for (int count = 0; count < N; count++)
                EndPoints[count] = new Point[2];
            bool[] bitRaised = new bool[N];
            int SilhouettesCount = 0;
            int i = 0;
            if (primitive.SilhouetteCurves.Length > 0)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[0].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[0].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }
            if (primitive.SilhouetteCurves.Length > 1)
            {
                EndPoints[i][0] = primitive.SilhouetteCurves[1].AssignedTo.Points[0];
                EndPoints[i][1] = primitive.SilhouetteCurves[1].AssignedTo.Points.Last();
                SilhouettesCount++;
                i++;
            }
            //PointsSequence FeatureCurve1 = new PointsSequence(); 
            if (primitive.FeatureCurves.Length > 0)
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
                    if (Math.Min(maxperimeter,v.Length) / Math.Max(maxperimeter,v.Length) < 0.7) ActiveFeatureCurve[0] = false;
                    i++;
                }
            }
            if (primitive.FeatureCurves.Length > 1)
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
                    i++;
                }
            }
  
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
                    /*ActiveCurves[counter].AssignedTo.isdeselected = true;
                    ActiveCurves[counter].AssignedTo = null;*/
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
