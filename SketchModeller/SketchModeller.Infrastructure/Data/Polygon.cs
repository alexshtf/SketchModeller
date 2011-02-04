using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class Polygon : PointsSequence
    {
        public Polygon(IEnumerable<Point> points = null)
            : base(points)
        {
        }
    }
}
