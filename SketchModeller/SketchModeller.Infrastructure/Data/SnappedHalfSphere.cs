using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class SnappedHalfSphere : SnappedPrimitive
    {
        public override SnappedPrimitive Clone()
        {
            return new SnappedHalfSphere();
        }
    }
}
