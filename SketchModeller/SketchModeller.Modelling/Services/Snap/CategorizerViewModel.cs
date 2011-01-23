using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Utils;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace SketchModeller.Modelling.Services.Snap
{
    public class CategorizerViewModel : NotificationObject
    {
        private PointsSequence[] selectedSequences;

        public CategorizerViewModel()
        {
            Finish = new DelegateCommand(FinishExecute, FinishCanExecute);
            Assign = new DelegateCommand(AssignExecute, AssignCanExecute);
            Result = new Dictionary<PointsSequence, Snapper.CurveCategory>();
            Categories = new ObservableCollection<Snapper.CurveCategory>();
            Sequences = new ObservableCollection<PointsSequence>();
        }

        public void Setup(IEnumerable<PointsSequence> sequences, Snapper.CurveCategory[] categories)
        {
            Contract.Requires(sequences != null);
            Contract.Requires(!sequences.IsEmpty());
            Contract.Requires(categories != null);
            Contract.Requires(categories.Length > 0);

            var points = sequences.SelectMany(x => x.Points).ToArray();
            MinX = points.Min(p => p.X);
            MaxX = points.Max(p => p.X);
            MinY = points.Min(p => p.Y);
            MaxY = points.Max(p => p.Y);

            Categories.Clear();
            Categories.AddRange(categories);

            if (CategoriesChanged != null)
                CategoriesChanged(this, EventArgs.Empty);

            Sequences.Clear();
            Sequences.AddRange(sequences);

            Result.Clear();
            IsFinished = false;

            ((DelegateCommand)Finish).RaiseCanExecuteChanged();
        }

        #region Communication with client

        public Dictionary<PointsSequence, Snapper.CurveCategory> Result { get; set; }

        #region IsFinished property

        private bool isFinished;

        internal bool IsFinished
        {
            get { return isFinished; }
            set
            {
                isFinished = value;
                RaisePropertyChanged(() => IsFinished);
            }
        }

        #endregion

        #endregion

        #region Communication with view

        public ICommand Finish { get; private set; }

        public ICommand Assign { get; private set; }

        public ObservableCollection<PointsSequence> Sequences { get; private set; }

        public ObservableCollection<Snapper.CurveCategory> Categories { get; private set; }

        #region SelectedCategory property

        private Snapper.CurveCategory selectedCategory;

        public Snapper.CurveCategory SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                RaisePropertyChanged(() => SelectedCategory);
                ((DelegateCommand)Assign).RaiseCanExecuteChanged();
            }
        }

        #endregion

        public event EventHandler AssignmentChanged;

        public event EventHandler CategoriesChanged;

        public event EventHandler Finished;

        public void UpdateSelection(PointsSequence[] sequences)
        {
            selectedSequences = sequences;

            if (sequences == null || sequences.Length == 0)
                SelectedCategory = null;
            else
            {
                var currentCategories =
                    from seq in sequences
                    select Result.ContainsKey(seq) ? Result[seq] : null;
                currentCategories = currentCategories.ToArray();
                
                var allSame = currentCategories.All(cat => cat == currentCategories.First());
                if (allSame)
                    SelectedCategory = currentCategories.First();
                else
                    SelectedCategory = null;
            }

            ((DelegateCommand)Assign).RaiseCanExecuteChanged();
        }

        #region MinX property

        private double minX;

        public double MinX
        {
            get { return minX; }
            set
            {
                minX = value;
                RaisePropertyChanged(() => MinX);
            }
        }

        #endregion

        #region MaxX property

        private double maxX;

        public double MaxX
        {
            get { return maxX; }
            set
            {
                maxX = value;
                RaisePropertyChanged(() => MaxX);
            }
        }

        #endregion

        #region MinY property

        private double miny;

        public double MinY
        {
            get { return miny; }
            set
            {
                miny = value;
                RaisePropertyChanged(() => MinY);
            }
        }

        #endregion

        #region MaxY property

        private double maxY;

        public double MaxY
        {
            get { return maxY; }
            set
            {
                maxY = value;
                RaisePropertyChanged(() => MaxY);
            }
        }

        #endregion

        #endregion

        #region Finish command methods

        private void FinishExecute()
        {
            IsFinished = true;
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        private bool FinishCanExecute()
        {
            // all sequences have been categorized
            return Sequences.Count == Result.Count;
        }

        #endregion

        #region Assign command methods

        private void AssignExecute()
        {
            foreach (var seq in selectedSequences)
                Result[seq] = SelectedCategory;

            if (AssignmentChanged != null)
                AssignmentChanged(this, EventArgs.Empty);

            ((DelegateCommand)Finish).RaiseCanExecuteChanged();
        }

        private bool AssignCanExecute()
        {
            return 
                SelectedCategory != null &&
                selectedSequences != null &&
                selectedSequences.Length > 0;
        }

        #endregion
    }
}
