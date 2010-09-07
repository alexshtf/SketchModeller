using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class MainViewModel : BaseViewModel
    {
        private double MIN_CURVE_LENGTH = 20;

        private readonly Func<string> chooseOpenFile;
        private readonly Func<string> chooseSaveFile;
        private List<Point> underConstructionCurve;
        private string lastSavedFile;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(underConstructionCurve != null);
            Contract.Invariant(
                Contract.ForAll(SketchCurvesViewModels, viewModel => CurveLength(viewModel.Curve.PolylinePoints) >= MIN_CURVE_LENGTH));
        }

        /// <summary>
        /// Constructs a new instance of the view model for the main window.
        /// </summary>
        /// <param name="chooseOpenFile"></param>
        /// <param name="chooseSaveFile"></param>
        public MainViewModel(Func<string> chooseOpenFile, Func<string> chooseSaveFile)
        {
            Contract.Requires(chooseOpenFile != null);
            Contract.Requires(chooseSaveFile != null);

            this.chooseOpenFile = chooseOpenFile;
            this.chooseSaveFile = chooseSaveFile;

            SketchCurvesViewModels = new ObservableCollection<SketchCurveViewModel>();
            underConstructionCurve = new List<Point>();

            MenuItems = new ObservableCollection<BaseMenuViewModel>
            {
                new MenuCategoryItem("File")
                {
                    MenuCommandItem.Create("Clear", o => Clear()),
                    MenuCommandItem.Create("Save", o => Save()),
                    MenuCommandItem.Create("Save as ...", o => SaveAs()),
                    MenuCommandItem.Create("Open ...", o => Open()),
                    MenuCommandItem.Create("Exit", o => Exit()),
                },
                new MenuCategoryItem("Help")
                {

                },
            };
        }

        #region Menu commands

        public void Clear()
        {
            underConstructionCurve = new List<Point>();
            NotifyUnderConstructionCurveChanged();

            SketchCurvesViewModels.Clear();
        }

        public void Save()
        {
            var fileName = lastSavedFile == null ? chooseSaveFile() : lastSavedFile;
            if (fileName != null) // we got a valid file name from the view
            {
                // TODO: Save a file
                lastSavedFile = fileName;
            }
        }

        public void SaveAs()
        {
            var fileName = chooseSaveFile();
            if (fileName != null)
            {
                // TODO: Save a file.
                lastSavedFile = fileName;
            }
        }

        public void Open()
        {
            var fileName = chooseOpenFile();
            if (fileName != null)
            {
                // TODO: Read stuff from a file.
                lastSavedFile = fileName;
            }
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Public properties and methods

        public ObservableCollection<BaseMenuViewModel> MenuItems { get; private set; }

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
            if (underConstructionCurve.Count > 1 && CurveLength(underConstructionCurve) >= MIN_CURVE_LENGTH)
            {
                var curveViewModel = new SketchCurveViewModel(underConstructionCurve);
                curveViewModel.CurveContextMenu.Add(MenuCommandItem.Create("Delete curve", o => DeleteCurve(curveViewModel)));
                SketchCurvesViewModels.Add(new SketchCurveViewModel(underConstructionCurve));
            }

            DiscardUnderConstructionCurve();
        }

        public void DeleteCurve(SketchCurveViewModel curveViewModel)
        {
            SketchCurvesViewModels.Remove(curveViewModel);
        }

        #endregion

        #region Private methods

        [Pure]
        private static double CurveLength(IEnumerable<Point> underConstructionCurve)
        {
            Contract.Requires(underConstructionCurve != null);
            Contract.Requires(underConstructionCurve.Count() >= 2);

            Contract.Ensures(Contract.Result<double>() >= 0);

            var lengths = from segment in underConstructionCurve.SeqPairs()
                          let p1 = segment.Item1
                          let p2 = segment.Item2
                          select (p2 - p1).Length;

            return lengths.Sum();
        }

        private void NotifyUnderConstructionCurveChanged()
        {
            NotifyPropertyChanged("UnderConstructionCurve");
        }

        #endregion
    }
     

}
