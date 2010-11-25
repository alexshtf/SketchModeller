using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure.Events;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SketchModeller.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Concurrency;

namespace SketchModeller.Modelling.Views
{
    public class OpenImageViewModel : NotificationObject
    {
        private IEventAggregator eventAggregator;
        private ISketchCatalog sketchCatalog;

        public OpenImageViewModel()
        {
            SketchNames = new ObservableCollection<string>();
            LoadSketchCommand = new DelegateCommand<string>(LoadSketchExecute);
        }

        [InjectionConstructor]
        public OpenImageViewModel(
            IEventAggregator eventAggregator,
            ISketchCatalog sketchCatalog)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;

            sketchCatalog.GetSketchNamesAsync().ObserveOnDispatcher().Subscribe(result =>
                {
                    SketchNames.Clear();
                    foreach (var name in result)
                        SketchNames.Add(name);
                });
        }

        public ObservableCollection<string> SketchNames { get; private set; }

        public ICommand LoadSketchCommand { get; private set; }

        private void LoadSketchExecute(string sketch)
        {
            eventAggregator.GetEvent<LoadSketchEvent>().Publish(sketch);
        }
    }
}
