using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using HelixToolkit;
using System.ComponentModel;
using Utils;
using System.Windows.Media;
using System.Windows.Input;
using Petzold.Media3D;
using System.Collections.ObjectModel;

namespace SketchModeller.Modelling.Views
{
    class NewSGCView : BaseNewPrimitiveView
    {
        private const int CIRCLE_DIV = 20;
        
        private readonly NewSGCViewModel viewModel;
        private readonly ModelVisual3D modelVisual;
        private readonly GeometryModel3D model;

        public NewSGCView(NewSGCViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += viewModel_PropertyChanged;

            model = new GeometryModel3D
            {
                Geometry = CreateGeometry(viewModel),
                Material = GetDefaultFrontMaterial(viewModel),
                BackMaterial = GetDefaultBackMaterial(),
            };
            modelVisual = new ModelVisual3D { Content = model };

            Children.Add(modelVisual);
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            model.Geometry = CreateGeometry(viewModel);
        }

        private static MeshGeometry3D CreateGeometry(NewSGCViewModel viewModel)
        {
            var startPoint = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis;
            var endPoint = viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis;
            var components = viewModel.Components;

            var path = from component in components
                       select MathUtils3D.Lerp(startPoint, endPoint, component.Progress);

            var diameters = from component in components
                            select 2 * component.Radius;

            var builder = new MeshBuilder();
            builder.AddTube(
                path.ToArray(),
                null,
                diameters.ToArray(),
                thetaDiv: CIRCLE_DIV,
                isTubeClosed: false);
            var geometry = builder.ToMesh(freeze: true);
            return geometry;
        }

        public override void DragStart(Point startPos, LineRange startRay)
        {
            base.DragStart(startPos, startRay);
        }

        protected override Vector3D ApproximateAxis
        {
            get { return viewModel.Axis; }
        }
    }
}
