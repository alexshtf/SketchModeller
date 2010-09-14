using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace MultiviewCurvesToCyl
{
    class SketchCurveViewModel : BaseViewModel
    {
        private bool isSelected;
        private int startIndex;
        private int endIndex;
        private ReadOnlyCollection<Point> annotatedPoints;
        private StartEndAnnotation startEndAnnotation;

        private readonly ObservableCollection<MenuCommandItem> startEndContextMenu;
        private readonly ReadOnlyObservableCollection<MenuCommandItem> startEndContextMenuReadOnly;

        private readonly ObservableCollection<MenuCommandItem> curveContextMenu;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(startIndex >= 0);
            Contract.Invariant(endIndex >= 0);
            Contract.Invariant(startIndex < Curve.PolylinePoints.Count);
            Contract.Invariant(endIndex < Curve.PolylinePoints.Count);
            Contract.Invariant(startIndex != endIndex);
        }

        public SketchCurveViewModel(IEnumerable<Point> polylinePoints)
        {
            Contract.Requires(polylinePoints != null);
            Contract.Requires(polylinePoints.Count() > 1);

            Curve = new SketchCurve(polylinePoints);
            startIndex = 0;
            endIndex = Curve.PolylinePoints.Count - 1;

            OnStartEndIndexChanged();

            startEndContextMenu = new ObservableCollection<MenuCommandItem>
            {
                new MenuCommandItem(new DelegateCommand(o => FlipStartEnd()), "Flip start and end"),
            };
            startEndContextMenuReadOnly = new ReadOnlyObservableCollection<MenuCommandItem>(startEndContextMenu);

            curveContextMenu = new ObservableCollection<MenuCommandItem>();
        }

        public SketchCurve Curve
        {
            get;
            private set;
        }

        /// <summary>
        /// All the points on the curve between start-index and end-index.
        /// </summary>
        public ReadOnlyCollection<Point> AnnotatedPoints
        {
            get { return annotatedPoints; }
            private set
            {
                annotatedPoints = value;
                NotifyPropertyChanged(() => AnnotatedPoints);
            }
        }

        public int StartIndex
        {
            get { return startIndex; }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Requires(value < Curve.PolylinePoints.Count);

                startIndex = value;
                NotifyPropertyChanged(() => StartIndex);
                OnStartEndIndexChanged();
            }
        }

        public int EndIndex
        {
            get { return endIndex; }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Requires(value < Curve.PolylinePoints.Count);

                endIndex = value;
                NotifyPropertyChanged(() => EndIndex);
                OnStartEndIndexChanged();
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyPropertyChanged(() => IsSelected);
            }
        }

        public ReadOnlyObservableCollection<MenuCommandItem> StartEndContextMenu
        {
            get { return startEndContextMenuReadOnly; }
        }

        public ObservableCollection<MenuCommandItem> CurveContextMenu
        {
            get { return curveContextMenu; }
        }

        public void FlipStartEnd()
        {
            SetStartEnd(newStartIndex: endIndex, newEndIndex: startIndex);
        }

        public void SetStartEnd(int newStartIndex, int newEndIndex)
        {

            startIndex = newStartIndex;
            endIndex = newEndIndex;

            NotifyPropertyChanged(() => StartIndex);
            NotifyPropertyChanged(() => EndIndex);

            OnStartEndIndexChanged();
        }

        #region change notification response

        private void OnStartEndIndexChanged()
        {
            var start = Math.Min(startIndex, endIndex);
            var end = Math.Max(startIndex, endIndex);

            AnnotatedPoints = Curve.PolylinePoints.Slice(start, end + 1).AsReadOnly();

            Curve.Annotations.Remove(startEndAnnotation);
            startEndAnnotation = new StartEndAnnotation
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
            };
            Curve.Annotations.Add(startEndAnnotation);
        }

        #endregion
    }
}
