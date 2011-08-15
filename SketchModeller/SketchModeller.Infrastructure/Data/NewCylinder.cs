using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewCylinder : NewCylindricalPrimitive
    {
        public ValueParameter Diameter { get; private set; }

        public double Radius
        {
            get { return Diameter / 2; }
        }

        protected override double BottomRadiusInternal
        {
            get { return Radius; }
        }

        protected override double TopRadiusInternal
        {
            get { return Radius; }
        }
    }
}
