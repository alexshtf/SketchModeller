using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Events;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SketchModeller.Infrastructure.Events;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Views
{
    public class EditMenuViewModel : NotificationObject
    {
        private readonly IEventAggregator eventAggregator;

        public EditMenuViewModel()
        {
            MarkFeature = new DelegateCommand(MarkFeatureExecute);
            MarkSilhouette = new DelegateCommand(MarkSilhouetteExecute);
            EnableInference = new DelegateCommand(EnableInferenceExecute);
            DisableInference = new DelegateCommand(DisableInferenceExecute);
        }

        [InjectionConstructor]
        public EditMenuViewModel(InferenceOptions inferenceOptions, IEventAggregator eventAggregator)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.InferenceOptions = inferenceOptions;
        }

        public ICommand MarkFeature { get; private set; }
        public ICommand MarkSilhouette { get; private set; }
        public ICommand EnableInference { get; private set; }
        public ICommand DisableInference { get; private set; }
        public InferenceOptions InferenceOptions { get; private set; }

        private void MarkFeatureExecute()
        {
            eventAggregator.GetEvent<MarkFeatureEvent>().Publish(null);
        }

        private void MarkSilhouetteExecute()
        {
            eventAggregator.GetEvent<MarkSilhouetteEvent>().Publish(null);
        }

        private void DisableInferenceExecute()
        {
            InferenceOptions.Cocentrality = false;
            InferenceOptions.CollinearCenters = false;
            InferenceOptions.CoplanarCenters = false;
            InferenceOptions.Coplanarity = false;
            InferenceOptions.OnSphere = false;
            InferenceOptions.OrthogonalAxes = false;
            InferenceOptions.Parallelism = false;
            InferenceOptions.SameRadius = false;
        }

        private void EnableInferenceExecute()
        {
            InferenceOptions.Cocentrality = true;
            InferenceOptions.CollinearCenters = true;
            InferenceOptions.CoplanarCenters = true;
            InferenceOptions.Coplanarity = true;
            InferenceOptions.OnSphere = true;
            InferenceOptions.OrthogonalAxes = true;
            InferenceOptions.Parallelism = true;
            InferenceOptions.SameRadius = true;
        }
    }
}
