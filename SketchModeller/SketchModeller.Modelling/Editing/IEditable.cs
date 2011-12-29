using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Editing
{
    public interface IEditable
    {
        Vector3D ApproximateAxis { get; }
        void NotifyDragged();
        SketchPlane SketchPlane { get; }
    }
}
