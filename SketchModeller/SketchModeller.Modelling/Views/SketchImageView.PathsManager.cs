using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using Utils;
using System.Windows.Input;
using System.Windows.Media;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchImageView
    {
        private class PathsManager
        {
            private readonly Canvas polyRoot;
            private readonly Rectangle selectionRectangle;

            private bool isSelecting;
            private ISet<Path> lastSelectionCandidates;
            private Point startPoint;
            private Point endPoint;

            public PathsManager(Canvas root, Rectangle selectionRectangle)
            {
                this.polyRoot = root;
                this.selectionRectangle = selectionRectangle;
                lastSelectionCandidates = EmptySet<Path>.Instance;
            }

            #region SelectionState attached property

            public static readonly DependencyProperty SelectionStateProperty =
                DependencyProperty.RegisterAttached("SelectionState", typeof(SelectionState), typeof(PathsManager));

            public static SelectionState GetSelectionState(Path target)
            {
                return (SelectionState)target.GetValue(SelectionStateProperty);
            }

            public static void SetSelectionState(Path target, SelectionState value)
            {
                target.SetValue(SelectionStateProperty, value);
            }

            #endregion

            #region Mouse events handling

            public void MouseDown(MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    UnselectAllPaths();

                    // when we start a selection process the last under-rect set is empty.
                    lastSelectionCandidates = EmptySet<Path>.Instance;

                    // set selection rectangle parameters to be the empty rectangle.
                    startPoint = e.GetPosition(polyRoot);
                    Canvas.SetLeft(selectionRectangle, startPoint.X);
                    Canvas.SetTop(selectionRectangle, startPoint.Y);
                    selectionRectangle.Width = 0;
                    selectionRectangle.Height = 0;
                    selectionRectangle.Visibility = Visibility.Visible;

                    polyRoot.CaptureMouse();

                    isSelecting = true;
                }
            }

            public void MouseMove(MouseEventArgs e)
            {
                if (isSelecting)
                {
                    endPoint = e.GetPosition(polyRoot);

                    var rect = new Rect(startPoint, endPoint);

                    Canvas.SetLeft(selectionRectangle, rect.Left);
                    Canvas.SetTop(selectionRectangle, rect.Top);
                    selectionRectangle.Width = rect.Width;
                    selectionRectangle.Height = rect.Height;

                    var currCandidates = FindPathsInRectangle();

                    var addedPaths = currCandidates.Except(lastSelectionCandidates);
                    foreach (var path in addedPaths)
                        SetSelectionState(path, SelectionState.Candidate);

                    var removedPaths = lastSelectionCandidates.Except(currCandidates);
                    foreach (var path in removedPaths)
                        SetSelectionState(path, SelectionState.Unselected);

                    lastSelectionCandidates = currCandidates;
                }
            }

            public void MouseUp(MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // hide selection visuals
                    polyRoot.ReleaseMouseCapture();
                    isSelecting = false;
                    selectionRectangle.Visibility = Visibility.Collapsed;

                    // perform the actual selection
                    foreach (var path in lastSelectionCandidates)
                    {
                        var pointsSequence = (SketchModeller.Infrastructure.Data.PointsSequence)path.DataContext;
                        pointsSequence.IsSelected = true;
                        SetSelectionState(path, SelectionState.Selected);
                    }
                }
            }

            #endregion

            #region Private utility methods

            private void UnselectAllPaths()
            {
                foreach (var path in polyRoot.VisualTree().OfType<Path>())
                    SetSelectionState(path, SelectionState.Unselected);
            }

            private ISet<Path> FindPathsInRectangle()
            {
                var rect = new Rect(startPoint, endPoint);
                var htParams = new GeometryHitTestParameters(new RectangleGeometry(rect));

                var hitTestResults = new HashSet<Path>();
                VisualTreeHelper.HitTest(
                    polyRoot,
                    filterCallback: null,
                    resultCallback: htResult =>
                    {
                        var path = htResult.VisualHit as Path;
                        if (path != null)
                        {
                            var geometryHtResult = (GeometryHitTestResult)htResult;
                            if (geometryHtResult.IntersectionDetail.HasFlag(IntersectionDetail.FullyInside))
                                hitTestResults.Add(path);
                        }
                        return HitTestResultBehavior.Continue;
                    },
                    hitTestParameters: htParams);

                return hitTestResults;
            }

            #endregion
        }

        private enum SelectionState
        {
            Unselected,
            Candidate,
            Selected,
        }
    }
}
