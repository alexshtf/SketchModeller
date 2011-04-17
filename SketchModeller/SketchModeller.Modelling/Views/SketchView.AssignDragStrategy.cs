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

namespace SketchModeller.Modelling.Views
{
    partial class SketchView
    {
        private class AssignDragStrategy : DragStrategyBase
        {
            private readonly ItemsControl primitiveCurvesRoot;
            private FrameworkElement lastEmphasized;

            public AssignDragStrategy(UiState uiState, ItemsControl primitiveCurvesRoot)
                : base(uiState)
            {
                this.primitiveCurvesRoot = primitiveCurvesRoot;
            }

            protected override void MouseDownCore(MousePosInfo3D position, object data)
            {
                throw new NotImplementedException();
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                EmphasizeCurves(position.Pos2D);
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                throw new NotImplementedException();
            }

            public void EmphasizeCurves(Point point)
            {
                if (lastEmphasized != null)
                    NewPrimitiveCurvesControl.SetIsEmphasized(lastEmphasized, false);

                var curve = FindNearbyCurve(point);
                if (curve != null)
                {
                    NewPrimitiveCurvesControl.SetIsEmphasized(curve, true);
                    lastEmphasized = curve;
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
