using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchToCyl
{
    struct SkeletonPoint
    {
        public Point3D Position { get; set; }
        public Vector3D Normal { get; set; }
        public double Radius { get; set; }
    }
}
