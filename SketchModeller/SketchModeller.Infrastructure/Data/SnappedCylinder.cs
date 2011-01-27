using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedCylinder : SnappedPrimitive
    {
        public Point3D[] TopCircle { get; set; }
        public Point3D[] BottomCircle { get; set; }
    }
}
