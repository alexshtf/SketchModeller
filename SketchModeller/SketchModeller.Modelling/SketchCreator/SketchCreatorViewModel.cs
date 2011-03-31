using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Input;

namespace SketchModeller.Modelling.SketchCreator
{
    public class SketchCreatorViewModel : NotificationObject
    {
        public SketchCreatorViewModel()
        {

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
            }
        }

        #endregion
    }
}
