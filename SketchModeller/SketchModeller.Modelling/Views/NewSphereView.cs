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
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Views
{
    class NewSphereView : BaseNewPrimitiveView
    {
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
            sphere.SetMaterials(GetDefaultFrontAndBackMaterials(viewModel));
        }

        protected override Vector3D ApproximateAxis
        {
            get { return new Vector3D(0,0,0); }
        }
    }
}
