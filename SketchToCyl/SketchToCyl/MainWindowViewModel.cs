using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Utils;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Diagnostics.Contracts;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace SketchToCyl
{
    class MainWindowViewModel : BaseViewModel
    {
        private readonly List<ToolbarCommand> toolbarCommands;
        private readonly ObservableCollection<List<Point>> strokes;
        private readonly ObservableCollection<List<Tuple<Point, Point>>> matchingPoints;
        private readonly ObservableCollection<Tuple<Point3D[], Vector3D[], int[]>> meshes;
        private List<Point> activeStroke;

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(activeStroke != null);
        }

        public MainWindowViewModel()
        {
            strokes = new ObservableCollection<List<Point>>();
            meshes = new ObservableCollection<Tuple<Point3D[], Vector3D[], int[]>>();
            matchingPoints = new ObservableCollection<List<Tuple<Point, Point>>>();

            toolbarCommands = new List<ToolbarCommand>
            {
                new ToolbarCommand("Clear", o => Clear()),
                new ToolbarCommand("Save curves", o => SaveToCSV()),
                new ToolbarCommand("Load curves", o => LoadFromCSV()),
                new ToolbarCommand("Curves average filter", o => AvgFilter()),
                new ToolbarCommand("Scatter matching points", o => FindMatchingPoints()),
                new ToolbarCommand("Create cylinder", o => CreateCylinder()),
            };

            activeStroke = new List<Point>();
        }

        public ReadOnlyCollection<ToolbarCommand> ToolbarCommands
        {
            get { return toolbarCommands.AsReadOnly(); }
        }

        public ObservableCollection<List<Point>> Strokes
        {
            get { return strokes; }
        }

        public ObservableCollection<List<Tuple<Point, Point>>> MatchingPoints
        {
            get { return matchingPoints; }
        }

        public ObservableCollection<Tuple<Point3D[], Vector3D[], int[]>> Meshes
        {
            get { return meshes; }
        }

        public ReadOnlyCollection<Point> ActiveStroke
        {
            get
            {
                return activeStroke.AsReadOnly();
            }
        }

        public void AddStroke()
        {
            if (activeStroke.Count > 2)
                strokes.Add(activeStroke);
            
            ClearActiveStroke();
        }

        public void AddPointToActiveStroke(Point pnt)
        {
            activeStroke.Add(pnt);
            NotifyPropertyChanged("ActiveStroke");
        }

        public void OnUpdateCloseStrokeInfo(Point projected, double projDist, int projIndex, int projStrokeIndex)
        {
            //var stroke = Strokes[projStrokeIndex];
            //if (projIndex > 0 && projIndex < stroke.Count - 1)
            //{
            //    var p1 = stroke[projIndex - 1];
            //    var p2 = stroke[projIndex];
            //    var p3 = stroke[projIndex + 1];
            //    var curvature = MathUtils.QuadraticCurvatureEstimate(p1, p2, p3);
            //    System.Diagnostics.Debug.WriteLine(curvature);
            //}
        }

        public event EventHandler<PromptFileEventArgs> PromptSaveCurves;

        public event EventHandler<PromptFileEventArgs> PromptLoadCurves;

        #region Commands

        [ContractVerification(true)]
        private void CreateCylinder()
        {
            if (matchingPoints.Count > 0)
            {
                var matches = matchingPoints[0];
                var skeletonWithoutLast =
                    (from match in matches.SeqPairs()
                     let curr = match.Item1
                     let next = match.Item2
                     let currMid = WpfUtils.Lerp(curr.Item1, curr.Item2, 0.5)
                     let nextMid = WpfUtils.Lerp(next.Item1, next.Item2, 0.5)
                     let normal = (nextMid - currMid).Normalized()
                     let radius = 0.5 * (curr.Item1 - curr.Item2).Length
                     select new SkeletonPoint
                     {
                         Position = new Point3D(currMid.X, currMid.Y, 0),
                         Radius = radius,
                         Normal = new Vector3D(normal.X, normal.Y, 0),
                     }).ToArray();

                var lastMatch = matches.Last();
                var lastMid = WpfUtils.Lerp(lastMatch.Item1, lastMatch.Item2, 0.5);
                var skeleton = skeletonWithoutLast.Append(new SkeletonPoint
                    {
                        Position = new Point3D(lastMid.X, lastMid.Y, 0),
                        Radius = (lastMatch.Item1 - lastMatch.Item2).Length * 0.5,
                        Normal = skeletonWithoutLast.Last().Normal,
                    });

                var mesh = SkeletonToMesh.SkeletonToCylinder(skeleton, 50);
                meshes.Add(mesh);
            }
        }

        [ContractVerification(true)]
        private void FindMatchingPoints()
        {
            if (strokes.Count != 2)
                return;

            var first  = strokes[0];
            var second = strokes[1];

            #region based on Kohonen SOM

            var matchesList = SOMPointsMatcher.MatchPoints(first, second, Math.Min(first.Count, second.Count) / 5);
            matchingPoints.Add(matchesList);

            #endregion

            #region based on graph matching

            //var dist = new double[first.Count, second.Count];
            //for (int i = 0; i < first.Count; ++i)
            //{
            //    for (int j = 0; j < second.Count; ++j)
            //        dist[i, j] = (first[i] - second[j]).Length;
            //}

            //var matches = MatchPointsWrapper.PointMatcher.MatchPoints(dist);
            //var matchesList = new List<Tuple<Point, Point>>();
            //for (int i = 0; i < first.Count; ++i)
            //{
            //    for (int j = 0; j < second.Count; ++j)
            //    {
            //        if (matches[i, j] > 0)
            //            matchesList.Add(Tuple.Create(first[i], second[j]));
            //    }
            //}

            //matchingPoints.Add(matchesList);
            #endregion
        }

        private static List<Point> GeneratePointsByLengthAndCurvature(double lengthThreshold, double curvatureFactor, List<Point> stroke, double[] innerCurvatures)
        {
            List<Point> result = new List<Point>();

            result.Add(stroke[0]);
            double currentMeasure = 0;
            for (int i = 1; i < stroke.Count - 1; ++i)
            {
                var prevPoint = stroke[i - 1];
                var currPoint = stroke[i];
                var segmentLength = (currPoint - prevPoint).Length;
                var segmentMeasure = segmentLength / (1 + curvatureFactor * Math.Abs(innerCurvatures[i - 1]));

                currentMeasure += segmentMeasure;
                if (currentMeasure >= lengthThreshold)
                {
                    currentMeasure = currentMeasure - lengthThreshold;
                    result.Add(currPoint);
                }
            }
            result.Add(stroke.Last());

            return result;
        }

        [ContractVerification(true)]
        private void Clear()
        {
            Strokes.Clear();
            ClearActiveStroke();
        }

        [ContractVerification(true)]
        private void SaveToCSV()
        {
            DoWithPromptFile(PromptSaveCurves, fileName =>
                {
                    using (var writer = new StreamWriter(fileName))
                    {
                        foreach (var point in strokes.ConcatWithSeperator(new Point(double.NaN, double.NaN)))
                            writer.WriteLine(string.Empty + point.X + ", " + point.Y);
                    }
                });
        }

        [ContractVerification(true)]
        private void LoadFromCSV()
        {
            DoWithPromptFile(PromptLoadCurves, fileName =>
                {
                    var points =
                        (from line in CsvReader.ReadCsv(fileName)
                         select line.ToPoint()).ToList();
                    var curves = points.Split(pnt => !double.IsNaN(pnt.X) && !double.IsNaN(pnt.Y));

                    strokes.Clear();
                    foreach (var curve in curves)
                        strokes.Add(new List<Point>(curve));
                });
        }

        [ContractVerification(true)]
        private void AvgFilter()
        {
            for (int i = 0; i < strokes.Count; i++)
            {
                var filteredStroke = CurveFilter.IterativeAverageFilter(strokes[i], 0.01 * strokes[i].Count);
                strokes[i] = new List<Point>(filteredStroke);
            }
        }

        #endregion

        private void DoWithPromptFile(EventHandler<PromptFileEventArgs> eventHandler, Action<string> action)
        {
            if (eventHandler != null)
            {
                var eventArgs = new PromptFileEventArgs();
                eventHandler(this, eventArgs);
                if (eventArgs.FileName != null)
                    action(eventArgs.FileName);
            }
        }

        private void ClearActiveStroke()
        {
            activeStroke = new List<Point>();
            NotifyPropertyChanged("ActiveStroke");
        }
    }
}
