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
            Uview = new Vector3D(1, 0, 0);
            Vview = new Vector3D(0, 1, 0);

            Components = new BendedCylinderComponent[]
            {
                new BendedCylinderComponent(1, 0, 0, 0),
                new BendedCylinderComponent(1, 1, 0, 0),
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

        public Vector3D Uview { get; set; }
        public Vector3D Vview { get; set; }
        public BendedCylinderComponent[] Components { get; set; }
    }
}
