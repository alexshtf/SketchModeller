﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class PrimitiveDragStrategy : DragStrategyBase
        {
            private readonly SketchModellingView sketchModellingView;
            private INewPrimitiveView draggedPrimitive;

            public PrimitiveDragStrategy(UiState uiState, SketchModellingView sketchModellingView)
                : base(uiState)
            {
                this.sketchModellingView = sketchModellingView;
            }

            protected override void MouseDownCore(MousePosInfo3D position, dynamic data)
            {
                draggedPrimitive = data.Item1 as INewPrimitiveView;
                if (draggedPrimitive != null && position.Ray3D != null)
                    draggedPrimitive.DragStart(position.Pos2D, position.Ray3D.Value);
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (draggedPrimitive != null && position.Ray3D != null)
                    draggedPrimitive.Drag(position.Pos2D, position.Ray3D.Value);
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (draggedPrimitive != null)
                    draggedPrimitive.DragEnd();
                draggedPrimitive = null;
            }
        }

    }
}
