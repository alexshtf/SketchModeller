using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace MultiviewCurvesToCyl.Base
{
    class Base3DViewModel : BaseViewModel, I3DViewModel
    {
        #region IsInWireframeMode property

        private bool isInWireframeMode;

        public bool IsInWireframeMode
        {
            get { return isInWireframeMode; }
            set
            {
                var oldVal = isInWireframeMode;
                isInWireframeMode = value;
                OnIsInWireframeModeChanged(oldVal, value);
            }
        }

        protected virtual void OnIsInWireframeModeChanged(bool oldVal, bool newVal)
        {
            NotifyPropertyChanged(() => IsInWireframeMode);
        }

        #endregion
    }
}
