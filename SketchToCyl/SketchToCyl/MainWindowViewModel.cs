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
        private List<Point> activeStroke;

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(activeStroke != null);
        }

        public MainWindowViewModel()
        {
            Point p1 = new Point(-3, 4);
            Point p2 = new Point(4, -2);
            Point p3 = new Point(5, 4);

            var fit = MathUtils.QuadraticFit(p1, p2, p3);
            System.Diagnostics.Debug.WriteLine("x(t) = {0}t²+{1}t+{2}", fit.Item1.X, fit.Item1.Y, fit.Item1.Z);
            System.Diagnostics.Debug.WriteLine("y(t) = {0}t²+{1}t+{2}", fit.Item2.X, fit.Item2.Y, fit.Item2.Z);


            strokes = new ObservableCollection<List<Point>>();
            matchingPoints = new ObservableCollection<List<Tuple<Point, Point>>>();

            toolbarCommands = new List<ToolbarCommand>
            {
                new ToolbarCommand("Clear", o => Clear()),
                new ToolbarCommand("Save curves", o => SaveToCSV()),
                new ToolbarCommand("Load curves", o => LoadFromCSV()),
                new ToolbarCommand("Curves average filter", o => AvgFilter()),
                new ToolbarCommand("Scatter matching points", o => FindMatchingPoints()),
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
