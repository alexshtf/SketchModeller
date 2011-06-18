using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public class NaivePointsSearchStructure
    {
        private readonly IList<Point> points;

        public NaivePointsSearchStructure(IList<Point> points)
        {
            Contract.Requires(points != null);
            this.points = points;
        }

        public int[] GetPointsInRect(Rect rect)
        {
            var query = from i in Enumerable.Range(0, points.Count)
                        where rect.Contains(points[i])
                        select i;

            return query.ToArray();
        }
    }
}
