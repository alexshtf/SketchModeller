using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Editing
{
    interface IEditorPrimitiveDuplicator
    {
        void CycleDuplicates(NewPrimitive originalDuplicate, ref NewPrimitive currentDuplicate, Vector3D currentDragVector);
        void UpdateDuplicatePosition(NewPrimitive originalDuplicate, ref NewPrimitive currentDuplicate, Vector3D currentDragVector);
        void DuplicateSnapped(SnappedPrimitive primitiveData, out NewPrimitive currentDuplicate, out NewPrimitive originalDuplicate);
    }
}
