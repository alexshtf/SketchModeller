using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using Petzold.Media3D;
using System.Windows;
using System.Diagnostics.Contracts;
using Utils;
using System.ComponentModel;
using SketchModeller.Infrastructure;
using SketchModeller.Utilities;
using System.Windows.Media;
using System.Windows.Input;

namespace SketchModeller.Modelling.Views
{
    class NewCylinderView : BaseNewPrimitiveView
    {
        private readonly NewCylinderViewModel viewModel;
        private readonly Cylinder cylinder;

        public NewCylinderView(NewCylinderViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            this.cylinder = new Cylinder();
            Children.Add(cylinder);

            cylinder.Bind(Cylinder.Radius1Property, () => viewModel.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Radius2Property, () => viewModel.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Point1Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center + 0.5 * length * axis);
            cylinder.Bind(Cylinder.Point2Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center - 0.5 * length * axis);

            cylinder.SetMaterials(GetDefaultFrontAndBackMaterials(viewModel));
        }
    }
}
