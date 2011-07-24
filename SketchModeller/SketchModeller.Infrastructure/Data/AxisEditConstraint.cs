using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class AxisEditConstraint : PrimitiveEditConstraint
    {
        public NewPrimitive AxisOf { get; set; }
    }
}
