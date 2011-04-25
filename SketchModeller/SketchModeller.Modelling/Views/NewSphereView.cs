using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using System.Windows.Input;
using Petzold.Media3D;
using Utils;

namespace SketchModeller.Modelling.Views
{
    class NewSphereView : BaseNewPrimitiveView
    {
        private const ModifierKeys RADIUS_MODIFIER = ModifierKeys.Shift;

        private readonly NewSphereViewModel viewModel;
        private readonly Sphere sphere;

        public NewSphereView(NewSphereViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            this.sphere = new Sphere();
            Children.Add(sphere);

            sphere.Bind(Sphere.RadiusProperty, () => viewModel.Radius);
            sphere.Bind(Sphere.CenterProperty, () => viewModel.Center);
            SetDefaultMaterial(sphere, viewModel);
        }

        protected override void PerformDrag(Vector dragVector2d, Vector3D dragVector3d, Vector3D axisDragVector, Point3D? sketchPlanePosition)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
                viewModel.Center = viewModel.Center + dragVector3d;
            if (Keyboard.Modifiers == RADIUS_MODIFIER)
            {
                if (sketchPlanePosition != null)
                {
                    var fromCenter = sketchPlanePosition.Value - viewModel.Center;
                    fromCenter.Normalize();
                    var radiusDelta = Vector3D.DotProduct(fromCenter, dragVector3d);
                    viewModel.Radius = Math.Max(viewModel.Radius + radiusDelta, NewSphereViewModel.MIN_RADIUS);
                }
            }
        }

        protected override Vector3D ApproximateAxis
        {
            get { return new Vector3D(0,0,0); }
        }
    }
}
