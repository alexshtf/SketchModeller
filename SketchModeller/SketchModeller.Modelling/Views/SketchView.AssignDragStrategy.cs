using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Utils;
using System.Windows.Media;
using SketchModeller.Infrastructure.Shared;
using System.Windows.Shapes;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Modelling.Events;
using SketchModeller.Modelling.Editing;

namespace SketchModeller.Modelling.Views
{
    partial class SketchView
    {
        private class AssignDragStrategy : DragStrategyBase
        {
            private readonly ItemsControl primitiveCurvesRoot;
            private readonly SketchImageView sketchImageView;
            private readonly IEventAggregator eventAggregator;
            private FrameworkElement lastEmphasizedPrimitiveCurve;
            private PointsSequence lastEmphasizedSketchCurve;

            public AssignDragStrategy(UiState uiState, ItemsControl primitiveCurvesRoot, SketchImageView sketchImageView, IEventAggregator eventAggregator)
                : base(uiState)
            {
                this.primitiveCurvesRoot = primitiveCurvesRoot;
                this.sketchImageView = sketchImageView;
                this.eventAggregator = eventAggregator;
            }

            public bool IsReadyToAssign
            {
                get { return lastEmphasizedPrimitiveCurve != null; }
            }

            protected override void MouseDownCore(MousePosInfo3D position, object data)
            {
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (IsDragging)
                    EmphasizeSketchCurves(position.Pos2D);
                else
                    EmphasizeCurves(position.Pos2D);
            }

            private void EmphasizeSketchCurves(Point point)
            {
                if (lastEmphasizedSketchCurve != null)
                    lastEmphasizedSketchCurve.IsEmphasized = false;

                var vec = new Vector(5, 5);
                var geometry = new RectangleGeometry(new Rect(point - vec, point + vec));
                var htParams = new GeometryHitTestParameters(geometry);

                lastEmphasizedSketchCurve = sketchImageView
                    .HitTestAll(htParams, dp => dp is Path)
                    .Cast<Path>()
                    .Select(x => x.DataContext)
                    .OfType<PointsSequence>()
                    .FirstOrDefault();

                if (lastEmphasizedSketchCurve != null)
                    lastEmphasizedSketchCurve.IsEmphasized = true;
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                EmphasizeCurves(position.Pos2D);

                if (lastEmphasizedSketchCurve != null)
                    lastEmphasizedSketchCurve.IsEmphasized = false;

                if (lastEmphasizedPrimitiveCurve != null)
                {
                    var primCurveData = NewPrimitiveCurvesControl.GetPrimitiveCurve(lastEmphasizedPrimitiveCurve);
                    primCurveData.IsUserAssignment = true;
                    primCurveData.AssignedTo = lastEmphasizedSketchCurve;
                    var npcControl = lastEmphasizedPrimitiveCurve.VisualPathUp().OfType<NewPrimitiveCurvesControl>().First();
                    eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Publish(npcControl.Primitive);
                }
                lastEmphasizedSketchCurve = null;
            }

            public void EmphasizeCurves(Point point)
            {
                if (lastEmphasizedPrimitiveCurve != null)
                    NewPrimitiveCurvesControl.SetIsEmphasized(lastEmphasizedPrimitiveCurve, false);

                var curve = FindNearbyCurve(point);
                if (curve != null)
                {
                    NewPrimitiveCurvesControl.SetIsEmphasized(curve, true);
                    lastEmphasizedPrimitiveCurve = curve;
                }
            }

            private FrameworkElement FindNearbyCurve(Point point)
            {
                var vec = new Vector(5, 5);
                var geometry = new RectangleGeometry(new Rect(point - vec, point + vec));
                var htParams = new GeometryHitTestParameters(geometry);
                
                var curve = (FrameworkElement)primitiveCurvesRoot.HitTestFirst(
                    htParams,
                    dp =>
                    {
                        var fwElement = dp as FrameworkElement;
                        return fwElement != null && NewPrimitiveCurvesControl.GetPrimitiveCurve(fwElement) != null;                        
                    });

                return curve;
            }
        }

    }
}
