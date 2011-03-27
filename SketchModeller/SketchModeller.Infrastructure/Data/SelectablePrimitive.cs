using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Data
{
    public class SelectablePrimitive : NotificationObject
    {
        #region IsSelected property

        [NonSerialized]
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        #endregion
    }
}
