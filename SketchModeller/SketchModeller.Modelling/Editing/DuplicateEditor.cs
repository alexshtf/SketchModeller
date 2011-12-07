using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Editing
{
    class DuplicateEditor : IDuplicateEditor
    {
        private IEditorPrimitiveDuplicator duplicator;
        private IDirectionInference directionInference;

        private NewPrimitive originalDuplicate;
        private NewPrimitive currentDuplicate;

        private Visual3D currentSnappedPrimitive;
        private Vector3D currentDragVector;

        public DuplicateEditor(IEditorPrimitiveDuplicator duplicator,
                               IDirectionInference directionInference)
        {
            this.duplicator = duplicator;
            this.directionInference = directionInference;
        }

        public void Start(Visual3D snappedPrimitive)
        {
            currentDragVector = new Vector3D(0, 0, 0);
            currentSnappedPrimitive = snappedPrimitive;
            originalDuplicate = null;
        }

        public void Update(MousePosInfo3D position, Vector vec2d, Vector3D vec3d)
        {
            if (vec3d != null && currentSnappedPrimitive != null)
            {
                currentDragVector += vec3d;
                DuplicateIfNecessary();
                UpdateNewPosition();
            }
        }

        public void Reset()
        {
            currentSnappedPrimitive = null;
            currentDuplicate = null;
            originalDuplicate = null;
        }

        public void CycleNext()
        {
            duplicator.CycleDuplicates(originalDuplicate, ref currentDuplicate, currentDragVector);
        }

        private void UpdateNewPosition()
        {
            duplicator.UpdateDuplicatePosition(originalDuplicate, ref currentDuplicate, currentDragVector);
        }

        private void DuplicateIfNecessary()
        {
            if (originalDuplicate == null)
            {
                var primitiveData = PrimitivesPickService.GetPrimitiveData(currentSnappedPrimitive) as SnappedPrimitive;
                duplicator.DuplicateSnapped(primitiveData, out currentDuplicate, out originalDuplicate);
            }
        }
    }
}
