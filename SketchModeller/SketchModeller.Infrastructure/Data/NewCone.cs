using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewCone : NewCylindricalPrimitive
    {
        public NewCone()
        {
            TopRadius = new ValueParameter();
            BottomRadius = new ValueParameter();

            RegisterParameter(() => TopRadius);
            RegisterParameter(() => BottomRadius);
        }

        public ValueParameter TopRadius { get; private set; }
        public ValueParameter BottomRadius { get; private set; }

        protected override double TopRadiusInternal
        {
            get { return TopRadius; }
        }

        protected override double BottomRadiusInternal
        {
            get { return BottomRadius; }
        }
    }
}
