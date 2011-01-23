using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class SnappedCylinder : SnappedPrimitive
    {
        public Point3D[] TopCircle { get; set; }
        public Point3D[] BottomCircle { get; set; }

        public override SnappedPrimitive Clone()
        {
            return new SnappedCylinder
            {
                TopCircle = this.TopCircle.Select(x => x.Clone()).ToArray(),
                BottomCircle = this.BottomCircle.Select(x => x.Clone()).ToArray(),
            };
        }
    }
}
