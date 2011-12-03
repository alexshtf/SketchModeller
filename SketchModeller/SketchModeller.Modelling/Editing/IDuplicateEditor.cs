using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace SketchModeller.Modelling.Editing
{
    interface IDuplicateEditor
    {
        void Start(Visual3D snappedPrimitive);
        void Update(MousePosInfo3D position, Vector vec2d, Vector3D vec3d);
        void CycleNext();
        void Reset();
    }
}
