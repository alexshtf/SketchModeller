using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Collections.ObjectModel;
using System.Windows;

namespace MultiviewCurvesToCyl
{
    class MainViewModel : BaseViewModel
    {
        private List<Point> underConstructionCurve;

        public MainViewModel()
        {
            SketchCurvesViewModels = new ObservableCollection<SketchCurveViewModel>();
            underConstructionCurve = new List<Point>();
        }

        /// <summary>
        /// A collection of view models for all the sketch curves.
        /// </summary>
        public ObservableCollection<SketchCurveViewModel> SketchCurvesViewModels { get; private set; }

        public ReadOnlyCollection<Point> UnderConstructionCurve
        {
            get { return underConstructionCurve.AsReadOnly(); }
        }

        public void AddUnderConstructionPoint(Point p)
        {
            underConstructionCurve.Add(p);
            NotifyUnderConstructionCurveChanged();
        }

        public void DiscardUnderConstructionCurve()
        {
            underConstructionCurve.Clear();
            NotifyUnderConstructionCurveChanged();
        }

        public void CommitUnderConstructionCurve()
        {
            if (underConstructionCurve.Count > 1)
                SketchCurvesViewModels.Add(new SketchCurveViewModel(underConstructionCurve));

            DiscardUnderConstructionCurve();
        }

        private void NotifyUnderConstructionCurveChanged()
        {
            NotifyPropertyChanged("UnderConstructionCurve");
        }
    }
}
