using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities.Graphs;
using Utils;

using Enumerable = System.Linq.Enumerable;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Computations
{
    /// <summary>
    /// Converts a single points cloud sampled from a polyline to a polyline that approximates it.
    /// </summary>
    static class PointsToPolylineConverter
    {
        /// <summary>
        /// Converts a set of points to a polyline
        /// </summary>
        /// <param name="points">The set of points</param>
        /// <param name="proximityThreshold">The threshold to consider to points "neighbors" on the polyline</param>
        /// <returns>A set of points that approximate the polyline given by <paramref name="points"/></returns>
        public static Point[] Convert(Point[] points, double proximityThreshold)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Length >= 1);

            Contract.Ensures(Contract.Result<Point[]>() != null);
            Contract.Ensures(Contract.Result<Point[]>().Length >= 1);

            Func<int, int, double> weight = (x, y) => (points[x] - points[y]).Length;
            var graph = GetPointsGraph(points, proximityThreshold);

            // first we find the farthest vertex from any vertex. This will be one endpoint of the path.
            var anyVertex = 0;
            var distances = Dijkstra.ComputeDistnces(graph, weight, anyVertex);
            var firstEndpoint = Enumerable.Range(0, distances.Length).Minimizer(i => -distances[i]);

            // now we compute the farthest point from the first endpoint. This will be the second endpoint of the path
            distances = Dijkstra.ComputeDistnces(graph, weight, firstEndpoint);
            var secondEndpoint = Enumerable.Range(0, distances.Length).Minimizer(i => -distances[i]);

            // now we compute the path between two endpoints
            var path = Dijkstra.ComputePath(graph, weight, firstEndpoint, secondEndpoint);

            // convert graph vertices of the path (indices of points) to points
            var result = from i in path
                         select points[i];

            return result.ToArray();
        }

        private static Tuple<int, int>[] GetPointsGraph(Point[] points, double proximityThreshold)
        {
            var searchStructure = new NaivePointsSearchStructure(points);
            var result =
                from i in Enumerable.Range(0, points.Length)
                let pnt = points[i]
                from j in FindNearPoints(pnt, searchStructure.GetPointsInRect, proximityThreshold)
                where i != j
                select Tuple.Create(i, j);

            return result.ToArray();
        }

        private static IEnumerable<int> FindNearPoints(Point pnt, Func<Rect, int[]> pointsInRect, double distance)
        {
            var result = new int[0];
            while (result.Length < 2)
            {
                var rect = new Rect(pnt - new Vector(distance, distance), pnt + new Vector(distance, distance));
                result = pointsInRect(rect);
                distance = distance * 1.5;
            }

            return result;
        }
    }
}
