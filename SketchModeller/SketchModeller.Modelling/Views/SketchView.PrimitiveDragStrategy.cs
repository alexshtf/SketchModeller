using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Modelling.Editing;
using SketchModeller.Infrastructure.Services;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class PrimitiveDragStrategy : DragStrategyBase
        {
            private readonly SketchModellingView sketchModellingView;
            private IEditor editor;
            private ISnapper snapper;

            private NewPrimitive draggedPrimitiveData;
            private ITemporarySnap temporarySnap;

            public PrimitiveDragStrategy(UiState uiState, SketchModellingView sketchModellingView, ISnapper snapper)
                : base(uiState)
            {
                this.sketchModellingView = sketchModellingView;
                this.snapper = snapper;
            }

            protected override void MouseDownCore(MousePosInfo3D position, dynamic data)
            {
                var draggedPrimitive = data.Item1 as INewPrimitiveView;
                if (draggedPrimitive != null && position.Ray3D != null)
                {
                    draggedPrimitiveData = draggedPrimitive.ViewModel.Model;
                    draggedPrimitive.OnStartEdit(position.Pos2D, position.Ray3D.Value);
                    editor = draggedPrimitive.ViewModel.StartEdit(position.Pos2D, position.Ray3D.Value);
                }
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (editor != null && position.Ray3D != null)
                {
                    editor.Drag(position.Pos2D, position.Ray3D.Value);

                    if (temporarySnap == null && draggedPrimitiveData != null && draggedPrimitiveData.CanSnap)
                        temporarySnap = snapper.TemporarySnap(draggedPrimitiveData);
                    else if (temporarySnap != null && draggedPrimitiveData.CanSnap) 
                        temporarySnap.Update();
                    else if (temporarySnap != null)
                    {
                        temporarySnap.Dispose();
                        temporarySnap = null;
                    }
                }
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                editor = null;
                if (temporarySnap != null) temporarySnap.Dispose();
                temporarySnap = null;
            }
        }

    }
}
