using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Windows;
using System.Collections.ObjectModel;

namespace SketchModeller.Infrastructure.Shared
{
    public class SessionData : NotificationObject
    {
        public SessionData()
        {
            NewPrimitives = new ObservableCollection<object>();
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

        public ObservableCollection<object> NewPrimitives { get; private set; }
    }

    public static class SessionDataExtensions
    {
        public static Size ImageSize(this SessionData sessionData)
        {
            if (sessionData.SketchData == null)
                return default(Size);

            var width = sessionData.SketchData.Image.GetLength(0);
            var height = sessionData.SketchData.Image.GetLength(1);
        
            return new Size(width, height);
        }
    }
}
