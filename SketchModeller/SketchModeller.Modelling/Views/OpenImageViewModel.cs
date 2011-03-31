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
using System.Diagnostics;
using SketchModeller.Modelling.Events;

namespace SketchModeller.Modelling.Views
{
    public class OpenImageViewModel : NotificationObject
    {
        private IEventAggregator eventAggregator;
        private ISketchCatalog sketchCatalog;
        private IUnityContainer container;

        public OpenImageViewModel()
        {
            SketchNames = new ObservableCollection<string>();
            CreateSketchCommand = new DelegateCommand(CreateSketchExecute);
            LoadSketchCommand = new DelegateCommand<string>(LoadSketchExecute);
            SaveSketchCommand = new DelegateCommand(SaveSketchExecute, SaveSketchCanExecute);
            TestCase = new DelegateCommand(TestCaseExecute);
        }

        [InjectionConstructor]
        public OpenImageViewModel(
            IEventAggregator eventAggregator,
            ISketchCatalog sketchCatalog,
            IUnityContainer container)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;
            this.container = container;
            RefreshSketches();
        }

        public ObservableCollection<string> SketchNames { get; private set; }

        public ICommand CreateSketchCommand { get; private set; }
        public ICommand LoadSketchCommand { get; private set; }
        public ICommand SaveSketchCommand { get; private set; }
        public ICommand TestCase { get; private set; }

        private void RefreshSketches()
        {
            sketchCatalog.GetSketchNamesAsync().ObserveOnDispatcher().Subscribe(result =>
            {
                SketchNames.Clear();
                foreach (var name in result)
                    SketchNames.Add(name);
            });
        }

        private void CreateSketchExecute()
        {
            var sketchCreator = container.Resolve<SketchCreator.SketchCreatorView>();
            sketchCreator.ShowDialog();
            RefreshSketches();
        }

        private void LoadSketchExecute(string sketch)
        {
            eventAggregator.GetEvent<LoadSketchEvent>().Publish(sketch);
        }

        private void SaveSketchExecute()
        {
            eventAggregator.GetEvent<SaveSketchEvent>().Publish(null);
        }

        private void TestCaseExecute()
        {
            eventAggregator.GetEvent<TestCaseEvent>().Publish(null);
        }

        private bool SaveSketchCanExecute()
        {
            return true;
        }
    }
}
