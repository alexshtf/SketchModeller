using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class PrimitiveDragStrategy : DragStrategyBase
        {
            private readonly SketchModellingView sketchModellingView;

            public PrimitiveDragStrategy(UiState uiState, SketchModellingView sketchModellingView)
                : base(uiState)
            {
                this.sketchModellingView = sketchModellingView;
            }

            protected override void MouseDownCore(MousePosInfo3D position)
            {
                SelectPrimitive(position);
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                DragPrimitive(position);
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                StopPrimitiveDragging();
            }

            private void StopPrimitiveDragging()
            {
                sketchModellingView.EndDrag();
            }

            private void SelectPrimitive(MousePosInfo3D positionInfo)
            {
                if (positionInfo.Ray3D != null)
                    sketchModellingView.SelectPrimitive(positionInfo.Pos2D, positionInfo.Ray3D.Value);
            }

            private void DragPrimitive(MousePosInfo3D positionInfo)
            {
                if (positionInfo.Ray3D != null)
                    sketchModellingView.DragPrimitive(positionInfo.Pos2D, positionInfo.Ray3D.Value);
            }
        }

    }
}
