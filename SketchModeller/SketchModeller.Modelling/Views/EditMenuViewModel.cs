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

namespace SketchModeller.Modelling.Views
{
    public class EditMenuViewModel : NotificationObject
    {
        private readonly IEventAggregator eventAggregator;

        public EditMenuViewModel()
        {
            MarkFeature = new DelegateCommand(MarkFeatureExecute);
            MarkSilhouette = new DelegateCommand(MarkSilhouetteExecute);
        }

        [InjectionConstructor]
        public EditMenuViewModel(IEventAggregator eventAggregator)
            : this()
        {
            this.eventAggregator = eventAggregator;
        }

        public ICommand MarkFeature { get; private set; }
        public ICommand MarkSilhouette { get; private set; }

        private void MarkFeatureExecute()
        {
            eventAggregator.GetEvent<MarkFeatureEvent>().Publish(null);
        }

        private void MarkSilhouetteExecute()
        {
            eventAggregator.GetEvent<MarkSilhouetteEvent>().Publish(null);
        }
    }
}
