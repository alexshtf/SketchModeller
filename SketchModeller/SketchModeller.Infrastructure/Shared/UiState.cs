using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Collections.ObjectModel;

namespace SketchModeller.Infrastructure.Shared
{
    public class UiState : NotificationObject
    {
        public UiState()
        {
            SketchPlane = SketchPlane.Default;
            SketchPlanes = new ObservableCollection<SketchPlane>() { SketchPlane };
        }

        #region SketchPlane property

        private SketchPlane sketchPlane;

        public SketchPlane SketchPlane
        {
            get { return sketchPlane; }
            set
            {
                sketchPlane = value;
                RaisePropertyChanged(() => SketchPlane);
            }
        }

        #endregion

        public ObservableCollection<SketchPlane> SketchPlanes { get; private set; }
     }
}
