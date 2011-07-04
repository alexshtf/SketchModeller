using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Utilities;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Utils;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Computations
{
    static class TwoLinesMedialAxis
    {
        private const int IMAGE_SPACE_WIDTH = 512;
        private const int IMAGE_SPACE_HEIGHT = 512;

        public const double PROXIMITY_DISTANCE = 3.0;

        public static Point[] Compute(Point[] l1, Point[] l2, Point[] polygon, double proximityDistance = PROXIMITY_DISTANCE)
        {
            var transformMatrix = GetTransformToImageSpace(l1, l2, polygon);

            // we are going to transform the arrays, so we copy them to not touch the original ones
            l1 = (Point[])l1.Clone();
            l2 = (Point[])l2.Clone();
            polygon = (Point[])polygon.Clone();

            // transform the arrays to image space coordinates
            transformMatrix.Transform(l1);
            transformMatrix.Transform(l2);
            transformMatrix.Transform(polygon);

            // compute distance transform of the polylines
            var distanceTransform = GetDistanceTransform(l1, l2);

            // find extreme points of the distance transform that lie inside the given polygon
            var medAxisPoints = FindExtremePoints(distanceTransform, polygon).ToArray();

            // transform the medial axis points back to original coordinates
            transformMatrix.Invert();
            transformMatrix.Transform(medAxisPoints);

            // filter extreme points (remove outliers)
            var filteredPoints = GetLargestCluster(medAxisPoints, threshold: proximityDistance);

            return filteredPoints;
        }

        private static Matrix GetTransformToImageSpace(params Point[][] lines)
        {
            // get sequences of X and Y coordinates (seperately) for all points
            var flat = lines.Flatten();
            var x = flat.Select(p => p.X);
            var y = flat.Select(p => p.Y);

            // use the above sequences to compute the bounding box (minX, minY) <--> (maxX, maxY)
            var maxX = x.Max();
            var minX = x.Min();
            var maxY = y.Max();
            var minY = y.Min();

            // compute transformation that normalizes the bounding box to image space
            // (minX, minY) --> (0,0)
            // (maxX, maxY) --> (IMAGE_SPACE_WIDTH - 1, IMAGE_SPACE_HEIGHT - 1)
            var translate = new Matrix(1, 0, 0, 1, -minX, -minY);
            var boxWidth = maxX - minX;
            var boxHeight = maxY - minY;
            var scale = new Matrix(IMAGE_SPACE_WIDTH / boxWidth, 0, 0, IMAGE_SPACE_HEIGHT / boxHeight, 0, 0);
            var result = translate * scale;

            // result is the computed matrix
            return result;
        }

        private static List<Point> FindExtremePoints(double[,] totalTransform, Point[] polygon)
        {
            var bitmask = PolygonRasterizer.Rasterize(polygon, IMAGE_SPACE_WIDTH, IMAGE_SPACE_HEIGHT);
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
                                where nRow >= 0 && nRow < IMAGE_SPACE_HEIGHT
                                from nCol in Enumerable.Range(col - 1, 3)
                                where nCol >= 0 && nCol < IMAGE_SPACE_WIDTH
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
            var transform1 = new double[IMAGE_SPACE_WIDTH, IMAGE_SPACE_HEIGHT];
            var transform2 = new double[IMAGE_SPACE_WIDTH, IMAGE_SPACE_HEIGHT];
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

            var clustersDescenging = from cluster in clustersDictionary.Values
                     orderby cluster.Count descending
                     select cluster;

            var largestCluster = clustersDictionary.Values.Any() 
                ? clustersDescenging.First() 
                : new List<Point>(0);

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
