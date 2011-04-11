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
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private readonly NewCylinderViewModel viewModel;
        private readonly Cylinder cylinder;

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

            SetDefaultMaterial(cylinder, viewModel);
        }

        protected override Vector3D ApproximateAxis
        {
            get { return viewModel.Axis; }
        }

        protected override void PerformDrag(Vector dragVector2d, Vector3D dragVector3d, Vector3D axisDragVector)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
                viewModel.Center = viewModel.Center + dragVector3d;
            else if (Keyboard.Modifiers == AXIS_MOVE_MODIFIER)
                viewModel.Center = viewModel.Center + axisDragVector;
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

        protected override CurvesInfo GetFeatureCurves()
        {
            var top = viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis;
            var topCircle = ShapeHelper.GenerateCircle(top, viewModel.Axis, 0.5 * viewModel.Diameter, 10);

            var bottom = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis;
            var bottomCircle = ShapeHelper.GenerateCircle(bottom, viewModel.Axis, 0.5 * viewModel.Diameter, 10);

            return new CurvesInfo
            {
                FeatureCurves = new Point3D[][] { topCircle, bottomCircle },
            };
        }
    }
}
