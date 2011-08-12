using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [Serializable]
    public class PointParameter : EditParameter<Point3D>
    {
        public override int Dimension
        {
            get { return 3; }
        }

        public override double[] GetValues()
        {
            return new double[] { Value.X, Value.Y, Value.Z };
        }

        public override void SetValues(double[] values)
        {
            Value = new Point3D(values[0], values[1], values[2]);
        }
    }
}
