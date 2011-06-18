using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Utilities;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Computations
{
    static class TwoLinesMedialAxis
    {
        public const double PROXIMITY_DISTANCE = 3.0;

        public static Point[] Compute(Point[] l1, Point[] l2, Point[] polygon, double proximityDistance = PROXIMITY_DISTANCE)
        {
            var distanceTransform = GetDistanceTransform(l1, l2);

            // rasterize the polygon to get the coordinates of pixels inside the polygon
            var medAxisPoints = FindExtremePoints(distanceTransform, polygon);

            // filter extreme points (remove outliers) and add first/last medial points
            var filteredPoints = GetLargestCluster(medAxisPoints, threshold: PROXIMITY_DISTANCE);

            return filteredPoints;
        }

        private static List<Point> FindExtremePoints(double[,] totalTransform, Point[] polygon)
        {
            var bitmask = PolygonRasterizer.Rasterize(polygon, 512, 512);
            Tuple<int[], int[]> nonZeros = GetNonZeros(bitmask);

            // find extreme points
            var nnzX = nonZeros.Item1;
            var nnzY = nonZeros.Item2;
            int nonZerosCount = nnzX.Length;
            var medAxisPoints = new List<Point>();
            for (int i = 0; i < nonZerosCount; i++)
            {
                var row = nnzX[i];
                var col = nnzY[i];
                var val = totalTransform[row, col];

                var neighbors = from nRow in Enumerable.Range(row - 1, 3)
                                from nCol in Enumerable.Range(col - 1, 3)
                                select new { Row = nRow, Col = nCol };
                var allInside = neighbors.All(x => bitmask[x.Row, x.Col] == true);

                if (allInside)
                {
                    var lowerNeighborsCount =
                        neighbors
                        .Where(x => val >= totalTransform[x.Row, x.Col])
                        .Count();

                    if (lowerNeighborsCount >= 7)
                        medAxisPoints.Add(new Point(row, col));
                }
            }
            return medAxisPoints;
        }

        private static double[,] GetDistanceTransform(Point[] l1, Point[] l2)
        {
            var transform1 = new double[512, 512];
            var transform2 = new double[512, 512];
            ChamferDistanceTransform.Compute(l1, transform1);
            ChamferDistanceTransform.Compute(l2, transform2);
            var totalTransform = Min(transform1, transform2);
            return totalTransform;
        }

        private static double[,] Min(double[,] transform1, double[,] transform2)
        {
            Contract.Requires(transform1.GetLength(0) == transform2.GetLength(0));
            Contract.Requires(transform1.GetLength(1) == transform2.GetLength(1));

            var width = transform1.GetLength(0);
            var height = transform1.GetLength(1);
            var result = new double[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result[x, y] = Math.Min(transform1[x, y], transform2[x, y]);

            return result;
        }

        private static Point[] GetLargestCluster(IList<Point> medAxisPoints, double threshold)
        {
            var unionFind = new IndexedUnionFind(medAxisPoints.Count);

            for (int i = 0; i < medAxisPoints.Count; ++i)
                for (int j = i + 1; j < medAxisPoints.Count; ++j)
                    if ((medAxisPoints[i] - medAxisPoints[j]).Length <= threshold)
                        unionFind.Union(i, j);

            var clustersDictionary = new Dictionary<int, List<Point>>();
            for (int i = 0; i < medAxisPoints.Count; ++i)
            {
                var setIndex = unionFind.Find(i);

                List<Point> cluster;
                if (!clustersDictionary.TryGetValue(setIndex, out cluster))
                    cluster = clustersDictionary[setIndex] = new List<Point>();

                cluster.Add(medAxisPoints[i]);
            }

            var largestCluster =
                (from cluster in clustersDictionary.Values
                 orderby cluster.Count descending
                 select cluster).First();

            return largestCluster.ToArray();
        }

        private static Tuple<int[], int[]> GetNonZeros(bool[,] bitmask)
        {
            var width = bitmask.GetLength(0);
            var height = bitmask.GetLength(1);

            var count = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (bitmask[x, y])
                        ++count;

            var resultX = new int[count];
            var resultY = new int[count];

            int globalIndex = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (bitmask[x, y])
                    {
                        resultX[globalIndex] = x;
                        resultY[globalIndex++] = y;
                    }

            return Tuple.Create(resultX, resultY);
        }
    }
}
