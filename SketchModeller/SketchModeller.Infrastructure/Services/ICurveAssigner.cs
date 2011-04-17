using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Infrastructure.Services
{
    public interface ICurveAssigner
    {
        void ComputeAssignments(NewPrimitive primitive);
    }
}
