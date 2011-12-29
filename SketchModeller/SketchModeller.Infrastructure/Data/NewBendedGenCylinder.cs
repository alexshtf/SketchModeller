using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewBendedGenCylinder : NewCylindricalPrimitive
    {
        public NewBendedGenCylinder()
        {
            Components = new BendedCylinderComponent[]
            {
                new BendedCylinderComponent(1, 0, new Point3D(0.0, 0.0, 0.0), new Point(0.0, 0.0)),
                new BendedCylinderComponent(1, 1, new Point3D(0.0, 0.0, 0.0), new Point(0.0, 0.0)),
            };
        }

        protected override double TopRadiusInternal
        {
            get { return Components.Last().Radius; }
        }

        protected override double BottomRadiusInternal
        {
            get { return Components.First().Radius; }
        }

        public BendedCylinderComponent[] Components { get; set; }
    }
}
