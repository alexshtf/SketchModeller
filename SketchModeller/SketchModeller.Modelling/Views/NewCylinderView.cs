using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using Petzold.Media3D;
using System.Windows;
using System.Diagnostics.Contracts;
using Utils;
using System.ComponentModel;
using SketchModeller.Infrastructure;
using SketchModeller.Utilities;
using System.Windows.Media;
using System.Windows.Input;

namespace SketchModeller.Modelling.Views
{
    class NewCylinderView : BaseNewPrimitiveView
    {
        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;

        private readonly NewCylinderViewModel viewModel;
        private readonly Cylinder cylinder;
        private bool isDragging;

        private Point3D? lastDragPosition3d;
        private Point lastDragPosition2d;

        public NewCylinderView(NewCylinderViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            this.cylinder = new Cylinder();
            Children.Add(cylinder);

            cylinder.Bind(Cylinder.Radius1Property, () => viewModel.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Radius2Property, () => viewModel.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Point1Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center + 0.5 * length * axis);
            cylinder.Bind(Cylinder.Point2Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center - 0.5 * length * axis);

            var material = new DiffuseMaterial();
            material.Bind(
                DiffuseMaterial.BrushProperty, 
                "Model.IsSelected", 
                viewModel, 
                new DelegateConverter<bool>(
                    isSelected =>
                    {
                        if (isSelected)
                            return SELECTED_BRUSH;
                        else
                            return UNSELECTED_BRUSH;
                    }));
            cylinder.Material = material;

            cylinder.BackMaterial = new DiffuseMaterial { Brush = Brushes.Red };
            cylinder.BackMaterial.Freeze();
        }

        public override void DragStart(Point startPos, LineRange startRay)
        {
            lastDragPosition3d = PointOnSketchPlane(startRay);
            lastDragPosition2d = startPos;
            isDragging = true;
        }

        public override void Drag(Point currPos, LineRange currRay)
        {
            var currDragPosition = PointOnSketchPlane(currRay);
            var dragVector3d = currDragPosition - lastDragPosition3d;
            var dragVector2d = currPos - lastDragPosition2d;

            if (dragVector3d != null)
                PerformDrag(dragVector2d, dragVector3d.Value);

            if (currDragPosition != null)
                lastDragPosition3d = currDragPosition;
            lastDragPosition2d = currPos;
        }

        public override void DragEnd()
        {
            isDragging = false;
        }

        public override bool IsDragging
        {
            get { return isDragging; }
        }

        private void PerformDrag(Vector dragVector2d, Vector3D dragVector3d)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
                viewModel.Center = viewModel.Center + dragVector3d;
            else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
            {
                viewModel.Axis = TrackballRotate(viewModel.Axis, dragVector2d);
            }
            else if (Keyboard.Modifiers == DIAMETER_MODIFIER)
            {
                var axis = Vector3D.CrossProduct(viewModel.Axis, viewModel.SketchPlane.Normal);
                if (axis != default(Vector3D))
                {
                    axis.Normalize();
                    var diameterDelta = Vector3D.DotProduct(axis, dragVector3d);
                    viewModel.Diameter = Math.Max(NewCylinderViewModel.MIN_DIAMETER, viewModel.Diameter + diameterDelta);
                }
            }
            else if (Keyboard.Modifiers == LENGTH_MODIFIER)
            {
                var axis = viewModel.Axis.Normalized();
                var lengthDelta = Vector3D.DotProduct(axis, dragVector3d) * 2;
                viewModel.Length = Math.Max(NewCylinderViewModel.MIN_LENGTH, viewModel.Length + lengthDelta);
            }
        }

        private Vector3D TrackballRotate(Vector3D toRotate, Vector dragVector2d)
        {
            const double TRACKBALL_ROTATE_SPEED = 1.0;

            var horzDegrees = -dragVector2d.X * TRACKBALL_ROTATE_SPEED;
            var vertDegrees = -dragVector2d.Y * TRACKBALL_ROTATE_SPEED;

            var horzAxis = viewModel.SketchPlane.Normal;
            var vertAxis = viewModel.SketchPlane.XAxis;

            toRotate = RotationHelper.RotateVector(toRotate, horzAxis, horzDegrees);
            toRotate = RotationHelper.RotateVector(toRotate, vertAxis, vertDegrees);
            return toRotate;
        }

        private Point3D? PointOnSketchPlane(LineRange lineRange)
        {
            var sketchPlane = viewModel.SketchPlane;
            return sketchPlane.PointFromRay(lineRange);
        }
    }
}
