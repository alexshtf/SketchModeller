using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using SketchModeller.Utilities;

using Enumerable = System.Linq.Enumerable;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Computations
{
    static class BendedSpine
    {
        public static Tuple<Point[], double[]> Compute(Point[] l1pts, Point[] l2pts, double[] progress)
        {
            //MessageBox.Show(String.Format("Number of Points={0}", l1pts.Length));
            //MessageBox.Show(String.Format("Number of Points={0}", l2pts.Length));
            //if (l1pts.Length < 5) l1pts = Chaikin(l1pts);
            //if (l2pts.Length < 5) l2pts = Chaikin(l2pts);

            //MessageBox.Show(String.Format("Number of Points={0}", l1pts.Length));
            //MessageBox.Show(String.Format("Number of Points={0}", l2pts.Length));

            Point[] Q1 = FourthDegreeBSplineApproximatin.FindBSplineApproximation(l1pts, 0);
            //MessageBox.Show("Completion of l1pts");
            Point[] Q2 = FourthDegreeBSplineApproximatin.FindBSplineApproximation(l2pts, 0);
            //MessageBox.Show("Completion of l2pts");
            Point[] Q3 = new Point[Q1.Length];
            Vector Chord1 = new Vector(Q2[0].X - Q1[Q1.Length - 1].X, Q2[0].Y - Q1[Q1.Length - 1].Y);
            Vector Chord2 = new Vector(Q2[0].X - Q1[0].X, Q2[0].Y - Q1[0].Y);
            int reverse = (Chord1.Length > Chord2.Length) ? 0 : 1;
            for (int i = 0; i < Q1.Length; i++)
            {
                double min = 10e10;
                int idx = -1;
                for (int j = 0; j < Q2.Length; j++)
                {
                    Vector vq1q2 = new Vector(Q2[j].X - Q1[i].X, Q2[j].Y - Q1[i].Y);
                    double length = vq1q2.Length;
                    if (min > length)
                    {
                        min = length;
                        idx = j;
                    }
                }
                Q3[i] = new Point((Q1[i].X + Q2[reverse * (Q2.Length - i - 1) + (1 - reverse) * i].X) / 2.0, (Q1[i].Y + Q2[reverse * (Q2.Length - i - 1) + (1 - reverse) * i].Y) / 2.0);
            }
            //MessageBox.Show("Starting medial axis approximation");
            Point[] medial_axis = FourthDegreeBSplineApproximatin.FindBSplineApproximation(Q3, 0, progress);
            //MessageBox.Show("Completion of medial axis approximation");

            List<double> ListRadii = new List<double>();
            for (int i = 0; i < medial_axis.Length; i++)
            {
                Vector v1 = new Vector(medial_axis[i].X, medial_axis[i].Y);
                double min = 10e10;
                double Radius = 0.0;
                for (int j = 0; j < Q1.Length; j++)
                {
                    Vector v2 = new Vector(Q1[j].X, Q1[j].Y);
                    Vector vm = v1 - v2;
                    Vector vspine = new Vector();
                    if (i < medial_axis.Length - 1)
                        vspine = new Vector(medial_axis[i].X - medial_axis[i + 1].X, medial_axis[i].Y - medial_axis[i + 1].Y);
                    else
                        vspine = new Vector(medial_axis[i].X - medial_axis[i - 1].X, medial_axis[i].Y - medial_axis[i - 1].Y);
                    double product = Math.Abs(vm * vspine);
                    if (min > product)
                    {
                        min = product;
                        Radius = vm.Length;
                    }
                }
                ListRadii.Add(Radius);
            }
            return Tuple.Create(medial_axis, ListRadii.ToArray());
        }
        static private Point[] Chaikin(Point[] pnts)
        {
            int n = pnts.Length;
            Point[] SubdivisionResult = pnts;
            while (n <= 90){
                List<Point> SubdivisionList = new List<Point>();
                SubdivisionList.Add(SubdivisionResult[0]);
                SubdivisionList.Add(new Point(1.0/4*SubdivisionResult[0].X + 3.0/4*SubdivisionResult[1].X, 
                                              1.0/4*SubdivisionResult[0].Y + 3.0/4*SubdivisionResult[1].Y));
                for (int i = 1; i < n-1; i++){
                    SubdivisionList.Add(new Point(3.0 / 4 * SubdivisionResult[i].X + 1.0 / 4 * SubdivisionResult[i+1].X,
                                                  3.0 / 4 * SubdivisionResult[i].Y + 1.0 / 4 * SubdivisionResult[i+1].Y));
                    SubdivisionList.Add(new Point(1.0 / 4 * SubdivisionResult[i].X + 3.0 / 4 * SubdivisionResult[i+1].X,
                                                  1.0 / 4 * SubdivisionResult[i].Y + 3.0 / 4 * SubdivisionResult[i+1].Y));
                }
                SubdivisionList.Add(new Point(3.0 / 4 * SubdivisionResult[n-2].X + 1.0 / 4 * SubdivisionResult[n - 1].X,
                                              3.0 / 4 * SubdivisionResult[n-2].Y + 1.0 / 4 * SubdivisionResult[n - 1].Y));
                SubdivisionList.Add(SubdivisionResult[n-1]);
                SubdivisionResult = SubdivisionList.ToArray();
                n *= 2;
            }
            return SubdivisionResult;
        }
    }
}
