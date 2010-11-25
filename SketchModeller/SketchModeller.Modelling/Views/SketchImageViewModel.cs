using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Services;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using System.ComponentModel;
using Utils;

namespace SketchModeller.Modelling.Views
{
    public class SketchImageViewModel : NotificationObject, System.Windows.IWeakEventListener
    {
        private ISketchCatalog sketchCatalog;
        private IEventAggregator eventAggregator;
        private DisplayOptions displayOptions;

        public SketchImageViewModel()
        {
        }

        [InjectionConstructor]
        public SketchImageViewModel(ISketchCatalog sketchCatalog, IEventAggregator eventAggregator, DisplayOptions displayOptions)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;
            eventAggregator.GetEvent<LoadSketchEvent>().Subscribe(LoadSketch);
            this.displayOptions = displayOptions;
            SetupDisplayOptionsSync();
        }

        #region ImageData property

        private double[,] imageData;

        public double[,] ImageData
        {
            get { return imageData; }
            private set
            {
                imageData = value;
                RaisePropertyChanged(() => ImageData);
            }
        }

        #endregion

        #region Points property

        private Point[] points;

        public Point[] Points
        {
            get { return points; }
            private set
            {
                points = value;
                RaisePropertyChanged(() => Points);
            }
        }

        #endregion

        #region IsImageShown property

        private bool isImageShown;

        public bool IsImageShown
        {
            get { return isImageShown; }
            private set
            {
                isImageShown = value;
                RaisePropertyChanged(() => IsImageShown);
            }
        }

        #endregion

        #region IsSketchShown property

        private bool isSketchShown;

        public bool IsSketchShown
        {
            get { return isSketchShown; }
            private set
            {
                isSketchShown = value;
                RaisePropertyChanged(() => IsSketchShown);
            }
        }

        #endregion

        private void LoadSketch(string fileName)
        {
            sketchCatalog.LoadSketchAsync(fileName).ObserveOnDispatcher().Subscribe(
                result => 
                { 
                    ImageData = result.Image;
                    Points = result.Points;
                },
                ex => eventAggregator.GetEvent<WorkingEvent>().Publish(false),
                () => eventAggregator.GetEvent<WorkingEvent>().Publish(false));
            eventAggregator.GetEvent<WorkingEvent>().Publish(true);
        }

        #region property sync related

        private string IsImageShownName
        {
            get { return PropertySupport.ExtractPropertyName(() => displayOptions.IsImageShown); }
        }

        private string IsSketchShownName
        {
            get { return PropertySupport.ExtractPropertyName(() => displayOptions.IsSketchShown); }
        }

        private void SetupDisplayOptionsSync()
        {
            PropertyChangedEventManager.AddListener(
                displayOptions, 
                this,
                IsImageShownName);

            PropertyChangedEventManager.AddListener(
                displayOptions,
                this,
                IsSketchShownName);

            IsImageShown = displayOptions.IsImageShown;
            IsSketchShown = displayOptions.IsSketchShown;
        }

        bool System.Windows.IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;

            if (eventArgs.PropertyName == IsSketchShownName)
                IsSketchShown = displayOptions.IsSketchShown;

            if (eventArgs.PropertyName == IsImageShownName)
                IsImageShown = displayOptions.IsImageShown;

            return true;
        }
        
        #endregion
    }
}
