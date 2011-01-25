using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SketchModeller.Infrastructure.Shared
{
    public class SessionData : NotificationObject
    {
        public SessionData()
        {
            NewPrimitives = new ObservableCollection<NewPrimitive>();
            SnappedPrimitives = new ObservableCollection<SnappedPrimitive>();
        }

        #region SketchData property

        private SketchData sketchData;

        /// <summary>
        /// The last loaded/saved sketch data.
        /// </summary>
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

        #region SketchName property

        private string sketchName;

        /// <summary>
        /// The currently loaded name.
        /// </summary>
        public string SketchName
        {
            get { return sketchName; }
            set
            {
                sketchName = value;
                RaisePropertyChanged(() => SketchName);
            }
        }

        #endregion

        public ObservableCollection<NewPrimitive> NewPrimitives { get; private set; }

        public ObservableCollection<SnappedPrimitive> SnappedPrimitives { get; private set; }
    }
}
