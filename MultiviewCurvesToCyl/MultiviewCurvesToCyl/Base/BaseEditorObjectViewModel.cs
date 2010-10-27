using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Collections.ObjectModel;

namespace MultiviewCurvesToCyl.Base
{
    class BaseEditorObjectViewModel : BaseViewModel
    {
        public BaseEditorObjectViewModel()
        {
            ContextMenu = new ObservableCollection<BaseMenuViewModel>();
        }

        public ObservableCollection<BaseMenuViewModel> ContextMenu { get; private set; }
    }
}
