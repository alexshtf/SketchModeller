using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class Polyline : PointsSequence
    {
        public Polyline()
            : base()
        {
        }

        public Polyline(IEnumerable<Point> points)
            : base(points)
        {
        }
    }
}
