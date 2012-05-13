using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class EnhancedPrimitiveCurve : PrimitiveCurve
    {
        public Point3D[] Points3D { get; set; }
    }
}
