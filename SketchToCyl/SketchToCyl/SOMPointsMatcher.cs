using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using System.Diagnostics.Contracts;

namespace SketchToCyl
{
    static class SOMPointsMatcher
    {
        public static List<Tuple<Point, Point>> MatchPoints(IEnumerable<Point> firstCurve, IEnumerable<Point> secondCurve, int numMatches)
        {
            Contract.Requires(firstCurve != null);
            Contract.Requires(secondCurve != null);
            Contract.Requires(numMatches >= 2);
            Contract.Requires(firstCurve.Count() >= numMatches);
            Contract.Requires(secondCurve.Count() >= numMatches);

            var n = firstCurve.Count();
            var m = secondCurve.Count();


            var som = new SOM<Point, Node>(numMatches * 2);
            
            // initialize r, c values of nodes
            for(int i = 0; i < 2 * numMatches; ++i)
            {
                som[i].Row = i % 2;
                som[i].Col = i / 2;
            }

            // initialize top and bottom rows to points scattered on the curves
            for (int i = 0; i < numMatches; ++i)
            {
                Func<int, int> relativeIdx = total => (int)((total - 1) * (i / (double)(numMatches - 1)));
                var fstIdx = relativeIdx(n);
                var sndIdx = relativeIdx(m);
                som[2 * i].Position = firstCurve.ElementAt(fstIdx);
                som[2*i + 1].Position = secondCurve.ElementAt(sndIdx);
            }

            som.Train(
                data: firstCurve.Concat(secondCurve),
                ops: new PointOps(),
                topoDistanceWeight: (n1, n2) => 1 / (1 + Math.Pow(Math.Abs(n1.Col - n2.Col) + 2 * Math.Abs(n1.Row - n2.Row), 3)),
                learningRates: Utils.Enumerable.Generate(0.1, alpha => 0.9 * alpha),
                threshold: 1);

            var result = new List<Tuple<Point, Point>>();
            for (int i = 0; i < numMatches; ++i)
            {
                var p1 = som[2 * i + 0].Position;
                var p2 = som[2 * i + 1].Position;
                var middle = (Point)(0.5 * (Vector)p1 + 0.5 * (Vector)p2);

                p1 = middle.ProjectionOnCurve(firstCurve).Item1;
                p2 = middle.ProjectionOnCurve(secondCurve).Item1;

                result.Add(Tuple.Create(p1, p2));
            }

            return result;
        }

        private class PointOps : IOperations<Point>
        {
            Point IOperations<Point>.Zero
            {
                get { return new Point(); }
            }

            Point IOperations<Point>.Add(Point v1, Point v2)
            {
                return (Point)((Vector)v1 + (Vector)v2);
            }

            Point IOperations<Point>.Scale(double scalar, Point v)
            {
                return (Point)(scalar * (Vector)v);
            }

            double IOperations<Point>.InnerProduct(Point v1, Point v2)
            {
                return (Vector)v1 * (Vector)v2;
            }
        }


        private class Node : ISOMNode<Point>
        {
            public Point Position { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
        }
    }
}
