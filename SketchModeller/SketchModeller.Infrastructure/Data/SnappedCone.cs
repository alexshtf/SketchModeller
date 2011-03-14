using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedCone : SnappedCylindricalPrimitive
    {
        public Variable BottomRadius { get; set; }
        public Variable TopRadius { get; set; }

        public double BottomRadiusResult { get; set; }
        public double TopRadiusResult { get; set; }
    }
}
