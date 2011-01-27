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
        public Polygon()
            : base()
        {
        }

        public Polygon(IEnumerable<Point> points)
            : base(points)
        {
        }
    }
}
