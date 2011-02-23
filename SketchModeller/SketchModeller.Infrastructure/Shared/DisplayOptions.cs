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
            IsSketchShown = true;
            IsSnappedPrimitivesShown = true;
            IsTemporaryPrimitivesShown = true;
        }

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

        #region IsSnappedPrimitivesShown property

        private bool isSnappedPrimitivesShown;

        public bool IsSnappedPrimitivesShown
        {
            get { return isSnappedPrimitivesShown; }
            set
            {
                isSnappedPrimitivesShown = value;
                RaisePropertyChanged(() => IsSnappedPrimitivesShown);
            }
        }

        #endregion

        #region IsTemporaryPrimitivesShown property

        private bool isTemporaryPrimitivesShown;

        public bool IsTemporaryPrimitivesShown
        {
            get { return isTemporaryPrimitivesShown; }
            set
            {
                isTemporaryPrimitivesShown = value;
                RaisePropertyChanged(() => IsTemporaryPrimitivesShown);
            }
        }

        #endregion
    }
}
