using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewStraightGenCylinder : NewCylindricalPrimitive
    {
        public NewStraightGenCylinder()
        {
            Components = new CylinderComponent[]
            {
                new CylinderComponent(0, 0),
                new CylinderComponent(0, 1),
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

        public CylinderComponent[] Components { get; set; }
    }
}
