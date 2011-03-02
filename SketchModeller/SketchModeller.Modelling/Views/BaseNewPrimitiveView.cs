using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Microsoft.Practices.Prism.Logging;
using Controls;
using Utils;
using System.Windows.Input;
using SketchModeller.Infrastructure;
using System.Windows.Controls;
using Petzold.Media3D;
using System.Diagnostics.Contracts;
using System.Windows;

namespace SketchModeller.Modelling.Views
{
    public abstract class BaseNewPrimitiveView : ModelVisual3D, INewPrimitiveView
    {
        // general fields
        private readonly NewPrimitiveViewModel viewModel;
        private readonly ILoggerFacade logger;

        public static readonly Brush UNSELECTED_BRUSH = Brushes.White;
        public static readonly Brush SELECTED_BRUSH = Brushes.LightBlue;

        protected readonly TranslateTransform3D translation;
        protected readonly ScaleTransform3D scale;
        protected readonly RotateTransform3D rotation;

        public BaseNewPrimitiveView(NewPrimitiveViewModel viewModel, ILoggerFacade logger)
        {
            this.viewModel = viewModel;
            this.logger = logger;

            translation = new TranslateTransform3D();
            scale = new ScaleTransform3D();
            rotation = new RotateTransform3D();

            Transform = new Transform3DGroup
            {
                Children = { rotation, scale, translation }
            };
        }

        NewPrimitiveViewModel INewPrimitiveView.ViewModel
        {
            get { return viewModel; }
        }

        public abstract void DragStart(LineRange startRay);

        public abstract void Drag(LineRange currRay);

        public abstract void DragEnd();

        public abstract bool IsDragging { get; }

    }
}
