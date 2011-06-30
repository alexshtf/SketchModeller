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

            modelVisual = new ModelVisual3D();
            model = new GeometryModel3D
            {
                Material = new DiffuseMaterial(Brushes.Cyan),
                BackMaterial = new DiffuseMaterial(Brushes.Red),
            };
            modelVisual.Content = model;
            model.Geometry = CreateGeometry(viewModel);
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            model.Geometry = CreateGeometry(viewModel);
        }

        private static MeshGeometry3D CreateGeometry(NewSGCViewModel viewModel)
        {
            var startPoint = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis;
            var endPoint = viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis;

            var path = from component in viewModel.Components
                       select MathUtils3D.Lerp(startPoint, endPoint, component.Progress);

            var diameters = from component in viewModel.Components
                            select 2 * component.Radius;

            var builder = new MeshBuilder();
            builder.AddTube(
                path.ToArray(),
                null,
                diameters.ToArray(),
                thetaDiv: CIRCLE_DIV,
                isTubeClosed: false);
            var geometry = builder.ToMesh(true);
            return geometry;
        }

        protected override void PerformDrag(
            Vector dragVector2d, 
            Vector3D dragVector3d, 
            Vector3D axisDragVector, 
            Point3D? sketchPlanePosition)
        {
            throw new NotImplementedException();
        }

        protected override Vector3D ApproximateAxis
        {
            get { return viewModel.Axis; }
        }
    }
}
