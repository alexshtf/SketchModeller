using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure;
using System.Collections.ObjectModel;
using SketchModeller.Infrastructure.Data;

using UiState = SketchModeller.Infrastructure.Shared.UiState;
using SketchPlane = SketchModeller.Infrastructure.Data.SketchPlane;

namespace SketchModeller.Modelling.Views
{
    public class NewPrimitiveViewModel : NotificationObject
    {
        protected UiState uiState;

        public NewPrimitiveViewModel(UiState uiState = null)
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
            this.uiState = uiState;
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }
        public NewPrimitive Model { get; set; }
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }
    }
}
