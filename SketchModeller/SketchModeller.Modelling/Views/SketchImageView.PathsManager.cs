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

using PointsSequence = SketchModeller.Infrastructure.Data.PointsSequence;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchImageView
    {
        private class PathsSelectionManager
        {
            private readonly Canvas polyRoot;
            private readonly Rectangle selectionRectangle;

            private bool isSelecting;
            private Point startPoint;
            private Point endPoint;

            public PathsSelectionManager(Canvas root, Rectangle selectionRectangle)
            {
                this.polyRoot = root;
                this.selectionRectangle = selectionRectangle;
            }

            #region Mouse events handling

            public void MouseDown(MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // set selection rectangle parameters to be the empty rectangle.
                    startPoint = e.GetPosition(polyRoot);
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
                }
            }

            public void MouseUp(MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left && isSelecting)
                {
                    // hide selection visuals
                    polyRoot.ReleaseMouseCapture();
                    isSelecting = false;
                    selectionRectangle.Visibility = Visibility.Collapsed;

                    endPoint = e.GetPosition(polyRoot);
                    var rect = new Rect(startPoint, endPoint);
                    var selection = FindPathsOverlapRectangle(rect);
                    UpdateSelection(selection);
                }
            }

            private void UpdateSelection(ISet<Path> selection)
            {
                if (Keyboard.Modifiers == ADD_MODIFIER)
                    AddToSelection(selection);
                else if (Keyboard.Modifiers == REMOVE_MODIFIER)
                    RemoveFromSelection(selection);
                else
                    ReplaceSelection(selection);
            }

            private void ReplaceSelection(ISet<Path> selection)
            {
                UnselectAllPaths();
                foreach (var ptsSequence in GetPointsSequences(selection))
                    ptsSequence.IsSelected = true;
            }

            private void RemoveFromSelection(ISet<Path> selection)
            {
                foreach (var ptsSequence in GetPointsSequences(selection))
                    ptsSequence.IsSelected = false;
            }

            private void AddToSelection(ISet<Path> selection)
            {
                foreach (var ptsSequence in GetPointsSequences(selection))
                    ptsSequence.IsSelected = true;
            }

            #endregion

            #region Private utility methods

            private IEnumerable<Path> GetAllPaths()
            {
                return polyRoot.VisualTree().OfType<Path>();
            }

            private void UnselectAllPaths()
            {
                var pointsSequences = GetPointsSequences(GetAllPaths());
                foreach (var ptsSequence in pointsSequences)
                    ptsSequence.IsSelected = false;
            }

            private IEnumerable<PointsSequence> GetPointsSequences(IEnumerable<Path> paths)
            {
                return paths.Select(x => x.DataContext).Cast<PointsSequence>();
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
    }
}
