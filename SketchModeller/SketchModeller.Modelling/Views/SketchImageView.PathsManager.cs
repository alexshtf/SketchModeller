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
        private class PathsSelectionManager
        {
            private readonly Canvas polyRoot;
            private readonly Rectangle selectionRectangle;

            private bool isSelecting;
            private ISet<Path> lastSelectionCandidates;
            private Point startPoint;
            private Point endPoint;

            public PathsSelectionManager(Canvas root, Rectangle selectionRectangle)
            {
                this.polyRoot = root;
                this.selectionRectangle = selectionRectangle;
                lastSelectionCandidates = EmptySet<Path>.Instance;
            }

            #region SelectionState attached property

            public static readonly DependencyProperty SelectionStateProperty =
                DependencyProperty.RegisterAttached("SelectionState", typeof(SelectionState), typeof(PathsSelectionManager));

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
                if (e.ChangedButton == MouseButton.Right && isSelecting) // we cancel selection process
                {
                    isSelecting = false;
                    polyRoot.ReleaseMouseCapture();
                    selectionRectangle.Visibility = Visibility.Collapsed;
                    foreach (var path in lastSelectionCandidates)
                        ClearSelectionStateFlags(path, SelectionState.Candidate);
                }
            }

            public void MouseMove(MouseEventArgs e)
            {
                ISet<Path> currCandidates;

                if (isSelecting)
                {
                    endPoint = e.GetPosition(polyRoot);
                    var rect = new Rect(startPoint, endPoint);

                    Canvas.SetLeft(selectionRectangle, rect.Left);
                    Canvas.SetTop(selectionRectangle, rect.Top);
                    selectionRectangle.Width = rect.Width;
                    selectionRectangle.Height = rect.Height;

                    currCandidates = FindPathsInsideRectangle(rect);
                }
                else // not isSelecting
                {
                    // rectangle around the current mouse position
                    var rect = new Rect(e.GetPosition(polyRoot), new Size(5, 5));
                    currCandidates = FindPathsOverlapRectangle(rect);
                }

                var addedPaths = currCandidates.Except(lastSelectionCandidates);
                foreach (var path in addedPaths)
                    SetSelectionStateFlag(path, SelectionState.Candidate);

                var removedPaths = lastSelectionCandidates.Except(currCandidates);
                foreach (var path in removedPaths)
                    ClearSelectionStateFlags(path, SelectionState.Candidate);

                lastSelectionCandidates = currCandidates;
            }

            public void MouseUp(MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left && isSelecting)
                {
                    UnselectAllPaths();

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

            private static void ClearSelectionStateFlags(Path path, SelectionState flags)
            {
                var selectionState = GetSelectionState(path);
                selectionState = selectionState & (~flags);
                SetSelectionState(path, selectionState);
            }

            private static void SetSelectionStateFlag(Path path, SelectionState flags)
            {
                var selectionState = GetSelectionState(path);
                selectionState = selectionState | flags;
                SetSelectionState(path, selectionState);
            }

            private IEnumerable<Path> GetAllPaths()
            {
                return polyRoot.VisualTree().OfType<Path>();
            }

            private void UnselectAllPaths()
            {
                foreach (var path in GetAllPaths())
                {
                    var pointsSequence = (SketchModeller.Infrastructure.Data.PointsSequence)path.DataContext;
                    pointsSequence.IsSelected = false;
                    SetSelectionState(path, SelectionState.Unselected);
                }
            }

            private ISet<Path> FindPathsInsideRectangle(Rect rect)
            {
                return FindPathsInRectangle(rect, IntersectionDetail.FullyInside);
            }

            private ISet<Path> FindPathsOverlapRectangle(Rect rect)
            {
                return FindPathsInRectangle(rect, IntersectionDetail.FullyInside, IntersectionDetail.Intersects);
            }

            private ISet<Path> FindPathsInRectangle(Rect rect, params IntersectionDetail[] intersectionDetail)
            {
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
                            foreach (var intersectionDetailItem in intersectionDetail)
                                if (geometryHtResult.IntersectionDetail.HasFlag(intersectionDetailItem))
                                    hitTestResults.Add(path);
                        }
                        return HitTestResultBehavior.Continue;
                    },
                    hitTestParameters: htParams);

                return hitTestResults;
            }

            #endregion
        }

        [Flags]
        private enum SelectionState
        {
            Unselected = 0,
            Candidate = 1,
            Selected = 2,
        }
    }
}
