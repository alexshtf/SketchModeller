using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedCylinder : SnappedCylindricalPrimitive
    {
        public Variable Radius { get; set; }
        public double RadiusResult { get; set; }
    }
}
