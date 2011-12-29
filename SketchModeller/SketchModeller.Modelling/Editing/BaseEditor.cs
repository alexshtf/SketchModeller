using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Petzold.Media3D;
using Utils;
using System.Windows.Media.Media3D;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Editing
{
    abstract class BaseEditor : IEditor
    {
        private readonly IEditable editable;
        private Point3D? lastDragPosition3d;
        private Point lastDragPosition2d;

        public BaseEditor(Point startPos, LineRange startRay, IEditable editable)
        {
            this.editable = editable;

            lastDragPosition3d = PointOnSketchPlane(startRay);
            lastDragPosition2d = startPos;
        }

        public void Drag(Point currPos, LineRange currRay)
        {
            var currDragPosition = PointOnSketchPlane(currRay);
            var dragVector3d = currDragPosition - lastDragPosition3d;
            var dragVector2d = currPos - lastDragPosition2d;

            if (dragVector3d != null)
            {
                var axisDragVector = MathUtils3D.ProjectVector(dragVector3d.Value, editable.ApproximateAxis);
                PerformDrag(dragVector2d, dragVector3d.Value, axisDragVector, currDragPosition);
                editable.NotifyDragged();
            }

            if (currDragPosition != null)
                lastDragPosition3d = currDragPosition;
            lastDragPosition2d = currPos;
        }

        private Point3D? PointOnSketchPlane(LineRange currRay)
        {
            var sketchPlane = editable.SketchPlane;
            return sketchPlane.PointFromRay(currRay);
        }

        protected abstract void PerformDrag(Vector dragVector2d, Vector3D vector3D, Vector3D axisDragVector, Point3D? currDragPosition);
    }
}
