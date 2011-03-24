using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class SnappedDragStrategy : DragStrategyBase
        {
            public SnappedDragStrategy(UiState uiState, SketchModellingView sketchModellingView, IEventAggregator eventAggregator)
                : base(uiState)
            {
            }

            protected override void MouseDownCore(MousePosInfo3D position)
            {
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
            }
        }
    }
}
