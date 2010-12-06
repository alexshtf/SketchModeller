using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure;
using System.Collections.ObjectModel;

namespace SketchModeller.Modelling.Views
{
    public class NewPrimitiveViewModel : NotificationObject
    {
        public NewPrimitiveViewModel()
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }
    }
}
