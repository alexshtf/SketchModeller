using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [Serializable]
    public class ValueParameter : EditParameter<double>
    {
        public override int Dimension
        {
            get { return 1; }
        }

        public override double[] GetValues()
        {
            return new double[] { Value };
        }

        public override void SetValues(double[] values)
        {
            Value = values[0];
        }
    }
}
