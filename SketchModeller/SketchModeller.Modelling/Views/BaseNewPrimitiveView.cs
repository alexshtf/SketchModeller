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
using SketchModeller.Utilities;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Views
{
    public abstract class BaseNewPrimitiveView : ModelVisual3D, INewPrimitiveView
    {
        // general fields
        private readonly NewPrimitiveViewModel viewModel;
        private readonly ILoggerFacade logger;

        public static readonly Brush UNSELECTED_BRUSH = new SolidColorBrush { Color = Colors.White, Opacity = 0.5 };
        public static readonly Brush SELECTED_BRUSH = new SolidColorBrush { Color = Colors.LightBlue, Opacity = 0.5 };

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
            PrimitivesPickService.SetPrimitiveData(this, viewModel.Model);
        }

        NewPrimitiveViewModel INewPrimitiveView.ViewModel
        {
            get { return viewModel; }
        }

        protected Tuple<Material, Material> GetDefaultFrontAndBackMaterials(NewPrimitiveViewModel viewModel)
        {
            return Tuple.Create(
                GetDefaultFrontMaterial(viewModel), 
                GetDefaultBackMaterial());
        }

        protected static Material GetDefaultBackMaterial()
        {
            var backMaterial = new DiffuseMaterial { Brush = new SolidColorBrush { Color = Colors.Red, Opacity = 0.5 } };
            backMaterial.Freeze();
            return backMaterial;
        }

        protected static Material GetDefaultFrontMaterial(NewPrimitiveViewModel viewModel)
        {
            var material = new DiffuseMaterial();
            material.Bind(
                DiffuseMaterial.BrushProperty,
                "Model.IsSelected",
                viewModel,
                new DelegateConverter<bool>(
                    isSelected =>
                    {
                        if (isSelected)
                            return SELECTED_BRUSH;
                        else
                            return UNSELECTED_BRUSH;
                    }));
            return material;
        }

        protected Point3D? PointOnSketchPlane(LineRange lineRange)
        {
            var sketchPlane = viewModel.SketchPlane;
            return sketchPlane.PointFromRay(lineRange);
        }

        public IEditor StartEdit(Point startPos, LineRange startRay)
        {
            return viewModel.StartEdit(startPos, startRay);
        }

        public virtual void OnStartEdit(Point startPos, LineRange startRay)
        {
        }
    }
}
