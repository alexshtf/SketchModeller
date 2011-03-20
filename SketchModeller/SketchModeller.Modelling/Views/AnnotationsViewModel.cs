using System;
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

namespace SketchModeller.Modelling.Views
{
    public class AnnotationsViewModel : NotificationObject
    {
        private readonly SessionData sessionData;
        private readonly ILoggerFacade logger;
        private readonly ISnapper snapper;

        public AnnotationsViewModel()
        {
            logger = new EmptyLogger();
            selectedAnnotationIndex = -1;

            RemoveCommand = new DelegateCommand(RemoveExecute, RemoveCanExecute);
            CoplanarCommand = new DelegateCommand(CoplanarExecute);
            ParallelCommand = new DelegateCommand(ParallelExecute);
            CocentricCommand = new DelegateCommand(CocentricExecue);
            Annotations = new ObservableCollection<Annotation>();
        }

        [InjectionConstructor]
        public AnnotationsViewModel(SessionData sessionData, ILoggerFacade logger, ISnapper snapper)
            : this()
        {
            this.sessionData = sessionData;
            this.logger = logger;
            this.snapper = snapper;
            Annotations = sessionData.Annotations;
        }

        public ICommand RemoveCommand { get; private set; }
        public ICommand CoplanarCommand { get; private set; }
        public ICommand ParallelCommand { get; private set; }
        public ICommand CocentricCommand { get; private set; }

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

        #endregion

        #region Helper methods

        private void SelectAnnotatedElements()
        {
            if (SelectedAnnotationIndex >= 0)
            {
                // unselect all sequences
                foreach (var item in sessionData.SketchObjects)
                    item.IsSelected = false;

                var currAnnotation = Annotations[SelectedAnnotationIndex];
                FeatureCurve[] elements = null;
                currAnnotation.MatchClass<Coplanarity>(coplarity => elements = coplarity.Elements);
                currAnnotation.MatchClass<Parallelism>(parallelism => elements = parallelism.Elements);
                currAnnotation.MatchClass<Cocentrality>(cocentrality => elements = cocentrality.Elements);
                Contract.Assume(elements != null);

                foreach (var ptsSequence in elements)
                    ptsSequence.IsSelected = true;
            }
        }

        private void AddAnnotation(Func<FeatureCurve[], Annotation> factory)
        {
            var selectedElements = sessionData.SelectedFeatureCurves.ToArray();
            if (selectedElements.Length > 0)
            {
                var annotation = factory(selectedElements);
                Annotations.Add(annotation);
                SelectedAnnotationIndex = Annotations.Count - 1;
                snapper.Recalculate();
            }
        }

        #endregion
    }
}
