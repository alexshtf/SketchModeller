﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using SketchModeller.Infrastructure.Data;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using Utils;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Modelling.Views
{
    public class AnnotationsViewModel : NotificationObject
    {
        private readonly IEventAggregator eventAggregator;
        private readonly SessionData sessionData;
        private readonly ILoggerFacade logger;
        private readonly ISnapper snapper;
        private readonly IUndoHistory undoHistory;

        public AnnotationsViewModel()
        {
            logger = new EmptyLogger();
            selectedAnnotationIndex = -1;

            RemoveCommand = new DelegateCommand(RemoveExecute, RemoveCanExecute);
            CoplanarCommand = new DelegateCommand(CoplanarExecute);
            ParallelCommand = new DelegateCommand(ParallelExecute);
            CocentricCommand = new DelegateCommand(CocentricExecue);
            ColinearCentersCommand = new DelegateCommand(ColinearCentersExecute);
            CoplanarCentersCommand = new DelegateCommand(CoplanarCentersExecute);
            OrthogonalAxesCommand = new DelegateCommand(OrthogonalAxesExecute);
            OnSphereCommand = new DelegateCommand(OnSphereExecute);
            Annotations = new ObservableCollection<Annotation>();
        }

        [InjectionConstructor]
        public AnnotationsViewModel(IEventAggregator eventAggregator, SessionData sessionData, ILoggerFacade logger, ISnapper snapper, IUndoHistory undoHistory)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sessionData = sessionData;
            this.logger = logger;
            this.snapper = snapper;
            this.undoHistory = undoHistory;
            Annotations = sessionData.Annotations;
        }

        public ICommand RemoveCommand { get; private set; }
        public ICommand CoplanarCommand { get; private set; }
        public ICommand ParallelCommand { get; private set; }
        public ICommand CocentricCommand { get; private set; }
        public ICommand ColinearCentersCommand { get; private set; }
        public ICommand CoplanarCentersCommand { get; private set; }
        public ICommand OrthogonalAxesCommand { get; private set; }
        public ICommand OnSphereCommand { get; private set; }

        public ObservableCollection<Annotation> Annotations { get; private set; }

        #region SelectedAnnotationIndex property

        private int selectedAnnotationIndex;

        public int SelectedAnnotationIndex
        {
            get { return selectedAnnotationIndex; }
            set
            {
                selectedAnnotationIndex = value;
                RaisePropertyChanged(() => SelectedAnnotationIndex);
                ((DelegateCommand)RemoveCommand).RaiseCanExecuteChanged();
                SelectAnnotatedElements();
            }
        }

        #endregion

        #region Commands

        private void RemoveExecute()
        {
            Annotations.RemoveAt(SelectedAnnotationIndex);
            Work.Execute(eventAggregator, () => snapper.RecalculateAsync());
        }

        private bool RemoveCanExecute()
        {
            return SelectedAnnotationIndex >= 0;
        }

        private void CoplanarExecute()
        {
            AddAnnotation(selectedElements => new Coplanarity { Elements = selectedElements });
        }

        private void ParallelExecute()
        {
            AddAnnotation(selectedElements => new Parallelism { Elements = selectedElements });
        }

        private void CocentricExecue()
        {
            AddAnnotation(selectedElements => new Cocentrality { Elements = selectedElements });
        }

        private void ColinearCentersExecute()
        {
            AddAnnotation(selectedElements => new ColinearCenters { Elements = selectedElements });
        }

        private void CoplanarCentersExecute()
        {
            AddAnnotation(selectedElements => new CoplanarCenters { Elements = selectedElements });
        }

        private void OrthogonalAxesExecute()
        {
            AddAnnotation(selectedElements => new OrthogonalAxis { Elements = selectedElements });
        }

        private void OnSphereExecute()
        {
            AddAnnotation(selectedElements => CreateOnSphereAnnotation(selectedElements));
        }

        #endregion

        #region Helper methods

        private Annotation CreateOnSphereAnnotation(FeatureCurve[] selectedElements)
        {
            // we can have only 2 feature curves (one of them belongs to a sphere)
            if (selectedElements.Length != 2)
                return null;

            // one of the feature curves must belong to a sphere and the other one must not
            CircleFeatureCurve onSphere;
            FeatureCurve other;
            if (!TryFindSingleFeatureCurveOnSphere(selectedElements, out onSphere, out other))
                return null;

            return new OnSphere 
            { 
                SphereOwned = onSphere, 
                CenterTouchesSphere = other,
                Elements = new FeatureCurve[] { onSphere, other },
            };
        }

        private bool TryFindSingleFeatureCurveOnSphere(FeatureCurve[] selectedElements, out CircleFeatureCurve onSphere, out FeatureCurve other)
        {
            onSphere = selectedElements.OfType<CircleFeatureCurve>().FirstOrDefault(IsOwnedBySphere);
            other = selectedElements.Where(x => !IsOwnedBySphere(x)).FirstOrDefault();

            return other != null && onSphere != null;
        }

        private bool IsOwnedBySphere(FeatureCurve featureCurve)
        {
            var owningSpheresCount = 
                sessionData.SnappedPrimitives
                .OfType<SnappedSphere>()
                .Count(sphere => sphere.FeatureCurves.Contains(featureCurve));

            return owningSpheresCount != 0;
        }

        private void SelectAnnotatedElements()
        {
            if (SelectedAnnotationIndex >= 0)
            {
                // unselect all feature curves
                var toUnSelect = sessionData.SelectedFeatureCurves.ToArray();
                foreach (var item in toUnSelect)
                    item.IsSelected = false;

                var currAnnotation = Annotations[SelectedAnnotationIndex];
                FeatureCurve[] elements = currAnnotation.Elements;
                Contract.Assume(elements != null);

                foreach (var featureCurve in elements)
                    featureCurve.IsSelected = true;
            }
        }

        private void AddAnnotation(Func<FeatureCurve[], Annotation> factory)
        {
            var selectedElements = sessionData.SelectedFeatureCurves.ToArray();
            if (selectedElements.Length > 0)
            {
                var annotation = factory(selectedElements);
                if (annotation != null) // null means that a valid annotation could not be created
                {
                    undoHistory.Push();
                    Annotations.Add(annotation);
                    SelectedAnnotationIndex = Annotations.Count - 1;
                    Work.Execute(eventAggregator, () => snapper.RecalculateAsync());
                }
            }
        }

        #endregion
    }
}
