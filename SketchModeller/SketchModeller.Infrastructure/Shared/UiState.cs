using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Infrastructure.Shared
{
    public class UiState : NotificationObject
    {
        public UiState()
        {
            Tool = Shared.Tool.Manipulation;
            SketchPlane = SketchPlane.Default;
        }

        #region Tool property

        private Tool tool;

        public Tool Tool
        {
            get { return tool; }
            set
            {
                tool = value;
                RaisePropertyChanged(() => Tool);
            }
        }

        #endregion

        #region SketchPlane property

        private SketchPlane sketchPlane;

        public SketchPlane SketchPlane
        {
            get { return sketchPlane; }
            set
            {
                sketchPlane = value;
                RaisePropertyChanged(() => SketchPlane);
            }
        }

        #endregion

        #region ImageWidth property

        private double imageWidth;

        public double ImageWidth
        {
            get { return imageWidth; }
            set
            {
                imageWidth = value;
                RaisePropertyChanged(() => ImageWidth);
            }
        }

        #endregion

        #region ImageHeight property

        private double imageHeight;

        public double ImageHeight
        {
            get { return imageHeight; }
            set
            {
                imageHeight = value;
                RaisePropertyChanged(() => ImageHeight);
            }
        }

        #endregion
    }
}
