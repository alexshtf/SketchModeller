using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Windows;

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

        #region SketchName property

        private string sketchName;

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
