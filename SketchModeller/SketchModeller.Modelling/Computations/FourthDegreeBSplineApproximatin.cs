using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Modelling.Computations
{
    static public class FourthDegreeBSplineApproximatin
    {
        public static int BinarySearch(double[] knot, double u)
        {
            int n = knot.Length;
            int Left = 0;
            int Right = n - 1;
            int middle = 0;
            while (Right - Left > 1)
            {
                middle = (Left + Right) / 2;
                if (u < knot[middle])
                    Right = middle;
                else
                    Left = middle;

            }
            return Left;
        }

        public static Point[] FindBSplineApproximation(Point[] pnts, int div, double[] progress )
        {
            int n = 4;
            Point[] Q = ReverseChaikin(pnts, div);
            int D = Q.Length;
            int K = D + n - 1;

            double[] u = GenerateKnotSequence(Q);
       
            List<Point> BSpline = new List<Point>();
            double[] utemp = (from i in Enumerable.Range(n - 1, K - n)
                              select u[i]).ToArray();
            foreach (double uk in progress)
            {
                //MessageBox.Show(String.Format("uk={0}", uk));
                double ukk = 0;
                if (uk == 1.0) ukk = uk-0.00001; 
                else ukk = uk; 
                int I = BinarySearch(utemp, ukk);
                //MessageBox.Show("Finished Binary Search");
                I += n - 1;
                BSpline.Add(deBoor(Q, I, u, ukk, n));
                //MessageBox.Show(String.Format("Completed"));
            }
            return BSpline.ToArray();
        
        }
            
        public static Point[] FindBSplineApproximation(Point[] pnts, int div = 4, double step = 0.01)
        {
            int n = 4;
            Point[] Q = ReverseChaikin(pnts, div);
            int D = Q.Length;
            int K = D + n - 1;

            double[] u = GenerateKnotSequence(Q);
            double delta = step;
            double Start = u[0];
            double Finish = u[u.Length - 1];
            double uk = Start;
            List<Point> BSpline = new List<Point>();
            double[] utemp = (from i in Enumerable.Range(n - 1, K - n)
                              select u[i]).ToArray();
            while (uk < Finish)
            {
                int I = BinarySearch(utemp, uk);
                I += n - 1;
                BSpline.Add(deBoor(Q, I, u, uk, n));
                uk += delta;
                if (uk - Finish >= 0.000001) uk = Finish - 0.000001;
            }
            return BSpline.ToArray();
        }
        private static Point deBoor(Point[] D, int I, double[] u, double uk, int n)
        {
            Vector[] PrevD = new Vector[D.Length];
            for (int i = 0; i < D.Length; i++) PrevD[i] = new Vector(D[i].X, D[i].Y);
            Vector[] NewD = PrevD;
            for (int k = 1; k <= n; k++)
            {
                for (int i = I - n + k + 1; i <= I + 1; i++)
                    NewD[i] = ((u[i + n - k] - uk) / (u[i + n - k] - u[i - 1])) * PrevD[i - 1] + ((uk - u[i - 1]) / (u[i + n - k] - u[i - 1])) * PrevD[i];
                PrevD = NewD;
            }
            return new Point(NewD[I + 1].X, NewD[I + 1].Y);
        }
        private static double[] GenerateKnotSequence(Point[] Q)
        {
            int n = 4;
            int D = Q.Length;
            int K = D + n - 1;
            double[] u = new double[K];
            for (int i = 0; i < n; i++) u[i] = 0.0;
            for (int i = n; i < K - n + 1; i++)
            {
                Vector v1 = new Vector(Q[i - n].X, Q[i - n].Y);
                Vector v2 = new Vector(Q[i - n + 1].X, Q[i - n + 1].Y);
                Vector v = v2 - v1;
                u[i] = u[i - 1] + v.Length;
            }
            for (int i = K - n + 1; i < K; i++) u[i] = u[K - n];
            for (int i = 1; i < K; i++) u[i] = (u[i] - u[0]) / (u[K - 1] - u[0]);
            return u;
        }

        private static Point[] ReverseChaikin(Point[] inPnts, int div)
        {
            Point[] tmpPnts = inPnts;
            for (int subCounter = 0; subCounter < div; subCounter++)
            {
                int n = tmpPnts.Length;
                List<Point> subPnt = new List<Point>();
                subPnt.Add(inPnts[0]);
                for (int i = 2; i < n; i += 2)
                {
                    if (i + 1 < n && i + 2 < n)
                    {
                        Vector[] P = new Vector[4];
                        for (int j = 0; j < 4; j++)
                            P[j] = new Vector(tmpPnts[i + j - 1].X, tmpPnts[i + j - 1].Y);
                        Vector Q = -0.25 * P[0] + (3.0 / 4) * P[1] + (3.0 / 4) * P[2] - 0.25 * P[2];
                        subPnt.Add(new Point(Q.X, Q.Y));
                    }
                }
                subPnt.Add(inPnts.Last());
                tmpPnts = subPnt.ToArray();
            }
            return tmpPnts;
        }
    }
}
