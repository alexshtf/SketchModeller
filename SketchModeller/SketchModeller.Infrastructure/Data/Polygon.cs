using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
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
