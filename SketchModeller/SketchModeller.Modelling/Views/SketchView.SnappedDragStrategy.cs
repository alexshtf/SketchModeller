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

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class SnappedDragStrategy : DragStrategyBase
        {
            private readonly SketchModellingView sketchModellingView;
            private readonly SketchViewModel sketchViewModel;
            private readonly SketchModellingViewModel sketchModellingViewModel;

            private NewPrimitive originalDuplicate;
            private NewPrimitive currentDuplicate;
            
            private Visual3D currentSnappedPrimitive;
            private Vector3D currentDragVector;

            public SnappedDragStrategy(
                UiState uiState, 
                SketchModellingView sketchModellingView,
                SketchViewModel sketchViewModel, 
                IEventAggregator eventAggregator)
                : base(uiState)
            {
                this.sketchModellingView = sketchModellingView;
                this.sketchViewModel = sketchViewModel;
                this.sketchModellingViewModel = sketchViewModel.SketchModellingViewModel;
                eventAggregator.GetEvent<GlobalShortcutEvent>().Subscribe(OnGlobalShortcut);
            }

            protected override void MouseDownCore(MousePosInfo3D position)
            {
                currentDragVector = new Vector3D(0, 0, 0);
                currentSnappedPrimitive = PickSnappedPrimitive(position);
                originalDuplicate = null;
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                if (vec3d != null && currentSnappedPrimitive != null)
                {
                    currentDragVector += vec3d.Value;
                    DuplicateIfNecessary();
                    UpdateNewPosition();
                }
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                currentSnappedPrimitive = null;
                currentDuplicate = null;
                originalDuplicate = null;
                //sketchViewModel.MouseInteractionMode = MouseInterationModes.PrimitiveManipulation;
            }

            private void OnGlobalShortcut(KeyEventArgs e)
            {
                if (e.Key == GlobalShortcuts.CyclePrimitives)
                    CycleNextPrimitive();
            }

            private Visual3D PickSnappedPrimitive(MousePosInfo3D position)
            {
                if (position.Ray3D != null)
                    return sketchModellingView.PickSnapped(position.Ray3D.Value);
                else
                    return null;
            }

            private void DuplicateIfNecessary()
            {
                if (originalDuplicate == null)
                {
                    var primitiveData = ModelViewerSnappedFactory.GetPrimitiveData(currentSnappedPrimitive);
                    sketchModellingViewModel.DuplicateSnapped(primitiveData, out currentDuplicate, out originalDuplicate);
                }
            }

            private void UpdateNewPosition()
            {
                sketchModellingViewModel.UpdateDuplicatePosition(originalDuplicate, ref currentDuplicate, currentDragVector);
            }

            private void CycleNextPrimitive()
            {
                sketchModellingViewModel.CycleDuplicates(originalDuplicate, ref currentDuplicate, currentDragVector);
            }
        }
    }
}
