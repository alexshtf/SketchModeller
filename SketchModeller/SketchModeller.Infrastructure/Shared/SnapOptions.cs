using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Shared
{
    public class SnapOptions : NotificationObject
    {
        public SnapOptions()
        {
            isSnapEnabled = true;
        }

        #region IsSnapEnabled property

        private bool isSnapEnabled;

        public bool IsSnapEnabled
        {
            get { return isSnapEnabled; }
            set
            {
                isSnapEnabled = value;
                RaisePropertyChanged(() => IsSnapEnabled);
            }
        }

        #endregion
    }
}
