using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;

namespace SketchModeller.Utilities
{
    public class NewCylinderWpf
    {
        public Point3D Center { get; set; }
        public Vector3D Axis { get; set; }
        public double Diameter { get; set; }
        public double Length { get; set; }
        
        public double Radius
        {
            get { return Diameter / 2; }
        }

        public Point3D Top
        {
            get { return Center + 0.5 * Length * Axis.Normalized(); }
        }

        public Point3D Bottom
        {
            get { return Center - 0.5 * Length * Axis.Normalized(); }
        }
    }
}
