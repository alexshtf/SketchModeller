using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using System.Windows.Controls;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class FeatureCurvesDragStrategy : DragStrategyBase
        {
            private readonly SketchModellingView sketchModellingView;
            private readonly FrameworkElement selectionRectangle;

            public FeatureCurvesDragStrategy(UiState uiState, FrameworkElement selectionRectangle, SketchModellingView sketchModellingView)
                : base(uiState)
            {
                this.sketchModellingView = sketchModellingView;
                this.selectionRectangle = selectionRectangle;
            }

            protected override void MouseDownCore(MousePosInfo3D position)
            {
                // we do nothing here
            }

            protected override void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                UpdateSelectionRectangle(position.Pos2D);
            }

            protected override void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d)
            {
                SelectCurves(position);
                selectionRectangle.Visibility = Visibility.Collapsed;
            }

            private void SelectCurves(MousePosInfo3D position)
            {
                // TODO: Write code here
            }

            private void UpdateSelectionRectangle(Point point)
            {
                var rect = new Rect(point, StartPosition.Pos2D);
                selectionRectangle.Width = rect.Width;
                selectionRectangle.Height = rect.Height;
                Canvas.SetTop(selectionRectangle, rect.Top);
                Canvas.SetLeft(selectionRectangle, rect.Left);
                selectionRectangle.Visibility = Visibility.Visible;
            }
        }

    }
}
