using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Shared
{
    public class DisplayOptions : NotificationObject
    {
        public DisplayOptions()
        {
            IsImageShown = true;
            IsSketchShown = true;
        }

        #region IsImageShown property

        private bool isImageShown;

        public bool IsImageShown
        {
            get { return isImageShown; }
            set
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
            set
            {
                isSketchShown = value;
                RaisePropertyChanged(() => IsSketchShown);
            }
        }

        #endregion
    }
}
