using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Events;
using System.Windows.Input;
using SketchModeller.Infrastructure;
using SketchModeller.Modelling.ModelViews;
using SketchModeller.Modelling.Editing;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class SnappedDragStrategy : DragStrategyBase
        {
            private readonly IDuplicateEditor duplicateEditor;
            private readonly SketchModellingView sketchModellingView;

            public SnappedDragStrategy(
                UiState uiState, 
                IDuplicateEditor duplicateEditor,
                IEventAggregator eventAggregator)
                : base(uiState)
            {
                eventAggregator.GetEvent<GlobalShortcutEvent>().Subscribe(OnGlobalShortcut);
            }

            protected override void MouseDownCore(MousePosInfo3D position, dynamic data)
            {
                Visual3D snappedPrimitive = data.Item1;
                duplicateEditor.Start(snappedPrimitive);
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (vec3d != null)
                    duplicateEditor.Update(position, vec2d, vec3d.Value);
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                duplicateEditor.Reset();
            }

            private void OnGlobalShortcut(KeyEventArgs e)
            {
                if (e.Key == GlobalShortcuts.CyclePrimitives)
                    duplicateEditor.CycleNext();
            }
        }
    }
}
