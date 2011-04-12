using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using System.Windows;
using System.ComponentModel;
using Utils;
using SketchModeller.Utilities;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using Petzold.Media3D;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace SketchModeller.Modelling.Views
{
    public class NewConeView : BaseNewPrimitiveView
    {
        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private readonly NewConeViewModel viewModel;
        private readonly Cylinder cylinder;
        private DragStartProximity dragStartProximity;

        [InjectionConstructor]
        public NewConeView(NewConeViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            cylinder = new Cylinder();
            Children.Add(cylinder);

            cylinder.Bind(Cylinder.Radius1Property, () => viewModel.TopRadius);
            cylinder.Bind(Cylinder.Radius2Property, () => viewModel.BottomRadius);
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

        public override void DragStart(Point startPos, LineRange startRay)
        {
            base.DragStart(startPos, startRay);
            dragStartProximity = GetDragStartProximity(startRay);
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
                    var radiusDelta = 0.5 * Vector3D.DotProduct(axis, dragVector3d);
                    if (dragStartProximity == DragStartProximity.Top)
                        viewModel.TopRadius = Math.Max(NewConeViewModel.MIN_DIAMETER, viewModel.TopRadius + radiusDelta);
                    else if (dragStartProximity == DragStartProximity.Bottom)
                        viewModel.BottomRadius = Math.Max(NewConeViewModel.MIN_DIAMETER, viewModel.BottomRadius + radiusDelta);
                }
            }
            else if (Keyboard.Modifiers == LENGTH_MODIFIER)
            {
                var axis = viewModel.Axis.Normalized();
                var lengthDelta = Vector3D.DotProduct(axis, dragVector3d) * 2;
                viewModel.Length = Math.Max(NewCylinderViewModel.MIN_LENGTH, viewModel.Length + lengthDelta);
            }
        }

        private DragStartProximity GetDragStartProximity(LineRange startRay)
        {
            DragStartProximity result = default(DragStartProximity);
            bool success = false;

            var htParams = new RayHitTestParameters(startRay.Point1, startRay.Point2 - startRay.Point1);
            var topNode = this.VisualPathUp().TakeWhile(x => x is Visual3D).OfType<Visual3D>().Last();

            VisualTreeHelper.HitTest(
                topNode,
                null,
                htResult =>
                {
                    if (htResult.VisualHit.VisualPathUp().Contains(cylinder))
                    {
                        var htResult3d = htResult as RayMeshGeometry3DHitTestResult;
                        var topPlane = Plane3D.FromPointAndNormal(viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis, viewModel.Axis);
                        var botPlane = Plane3D.FromPointAndNormal(viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis, viewModel.Axis);

                        var topDist = topPlane.DistanceFromPoint(htResult3d.PointHit);
                        var botDist = botPlane.DistanceFromPoint(htResult3d.PointHit);

                        if (topDist < botDist)
                            result = DragStartProximity.Top;
                        else
                            result = DragStartProximity.Bottom;

                        success = true;
                        return HitTestResultBehavior.Stop;
                    }
                    else
                        return HitTestResultBehavior.Continue;
                },
                htParams);

            Debug.Assert(success == true);
            return result;
        }

        private enum DragStartProximity
        {
            Top,
            Bottom,
        }

        protected override CurvesInfo GetFeatureCurves()
        {
            var top = viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis;
            var topCircle3d = ShapeHelper.GenerateCircle(top, viewModel.Axis, viewModel.TopRadius, 10);
            var topCircle = ProjectCurve(topCircle3d);

            var bottom = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis;
            var bottomCircle3d = ShapeHelper.GenerateCircle(bottom, viewModel.Axis, viewModel.BottomRadius, 10);
            var bottomCircle = ProjectCurve(bottomCircle3d);

            // find the axis in projected coordinates
            var tb = ProjectCurve(top, bottom);
            var axis2d = tb[0] - tb[1];

            // find the 2 silhouette lines
            var perp = new Vector(axis2d.Y, -axis2d.X);
            perp.Normalize();
            var lt = tb[0] + viewModel.TopRadius * perp;
            var lb = tb[1] + viewModel.BottomRadius * perp;
            var rt = tb[0] - viewModel.TopRadius * perp;
            var rb = tb[1] - viewModel.BottomRadius * perp;

            var leftLine = new Point[] { lt, lb };
            var rightLine = new Point[] { rt, rb };

            return new CurvesInfo
            {
                FeatureCurves = new Point[][] { topCircle, bottomCircle },
                SilhouetteCurves = new Point[][] { leftLine, rightLine },
            };
        }
    }
}
