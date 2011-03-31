using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.SketchCreator
{
    public class SketchCreatorViewModel : NotificationObject
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ISketchCatalog sketchCatalog;

        public SketchCreatorViewModel()
        {
            Curves = new ObservableCollection<PointsSequence>();
            isFeatureMode = true;
            SaveCommand = new DelegateCommand(SaveExecute);
        }

        [InjectionConstructor]
        public SketchCreatorViewModel(IEventAggregator eventAggregator, ISketchCatalog sketchCatalog)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand FeatureCommand { get; private set; }
        public ICommand SilhouetteCommand { get; private set; }

        public ObservableCollection<PointsSequence> Curves { get; private set; }
        
        #region IsFeatureMode property

        private bool isFeatureMode;

        public bool IsFeatureMode
        {
            get { return isFeatureMode; }
            set
            {
                isFeatureMode = value;
                RaisePropertyChanged(() => IsFeatureMode);
                if (IsSilhouetteMode && IsFeatureMode)
                    IsSilhouetteMode = false;
            }
        }

        #endregion

        #region IsSilhouetteMode property

        private bool isSilhouetteMode;

        public bool IsSilhouetteMode
        {
            get { return isSilhouetteMode; }
            set
            {
                isSilhouetteMode = value;
                RaisePropertyChanged(() => IsSilhouetteMode);
                if (IsFeatureMode && IsSilhouetteMode)
                    IsFeatureMode = false;
            }
        }

        #endregion

        #region Command execute

        private void SaveExecute()
        {
            Work.Execute(eventAggregator, () => sketchCatalog.CreateSketchAsync("newsketch",
                new SketchData
                {
                    Annotations = new Annotation[0],
                    NewPrimitives = new NewPrimitive[0],
                    Polygons = Curves.OfType<Polygon>().ToArray(),
                    Polylines = Curves.OfType<Polyline>().ToArray(),
                    SnappedPrimitives = new SnappedPrimitive[0],
                }));
        }

        #endregion
    }
}
