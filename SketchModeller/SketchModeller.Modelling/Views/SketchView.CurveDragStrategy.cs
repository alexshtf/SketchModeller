using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        private class CurveDragStrategy : DragStrategyBase
        {
            private readonly SketchImageView sketchImageView;
            private readonly FrameworkElement selectionRectangle;

            public CurveDragStrategy(UiState uiState, SketchImageView sketchImageView, FrameworkElement selectionRectangle)
                : base(uiState)
            {
                this.sketchImageView = sketchImageView;
                this.selectionRectangle = selectionRectangle;
            }

            protected override void MouseDownCore(MousePosInfo3D position, dynamic data)
            {
                // we do nothing in response to mouse down
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

            private void SelectCurves(MousePosInfo3D positionInfo)
            {
                var rect = new Rect(positionInfo.Pos2D, StartPosition.Pos2D);
                sketchImageView.SelectCurves(rect);
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
