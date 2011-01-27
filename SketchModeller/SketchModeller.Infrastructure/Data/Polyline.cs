using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class Polyline : PointsSequence
    {
        public Polyline(IEnumerable<Point> points)
            : base(points)
        {
        }
    }
}
