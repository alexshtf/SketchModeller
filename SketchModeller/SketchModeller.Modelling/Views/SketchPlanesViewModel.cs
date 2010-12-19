using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.ComponentModel;
using SketchModeller.Infrastructure.Shared;
using System.Windows.Data;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Prism.Logging;

namespace SketchModeller.Modelling.Views
{
    public class SketchPlanesViewModel : NotificationObject
    {
        private UiState uiState;
        private ILoggerFacade log;

        public SketchPlanesViewModel(UiState uiState, ILoggerFacade log)
        {
            this.uiState = uiState;
            this.log = log;

            SketchPlanes = CollectionViewSource.GetDefaultView(uiState.SketchPlanes);
            SketchPlanes.MoveCurrentTo(uiState.SketchPlane);
            SketchPlanes.CurrentChanged += OnSketchPlanesCurrentChanged;
        }

        private void OnSketchPlanesCurrentChanged(object sender, EventArgs e)
        {
            if (SketchPlanes.CurrentItem != null)
                uiState.SketchPlane = (SketchPlane)SketchPlanes.CurrentItem;
            else
                log.Log("CurrentItem is null, which is illegal", Category.Warn, Priority.High);
        }

        public ICollectionView SketchPlanes { get; private set; }
    }
}
