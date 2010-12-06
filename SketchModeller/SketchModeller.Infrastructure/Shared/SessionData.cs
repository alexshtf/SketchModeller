using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Infrastructure.Shared
{
    public class SessionData : NotificationObject
    {
        #region SketchData property

        private SketchData sketchData;

        public SketchData SketchData
        {
            get { return sketchData; }
            set
            {
                sketchData = value;
                RaisePropertyChanged(() => SketchData);
            }
        }

        #endregion
    }
}
