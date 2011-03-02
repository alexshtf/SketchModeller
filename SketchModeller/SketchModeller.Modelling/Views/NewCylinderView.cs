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

namespace SketchModeller.Modelling.Views
{
    class NewCylinderView : BaseNewPrimitiveView
    {
        private readonly NewCylinderViewModel viewModel;
        private readonly Cylinder cylinder;
        private bool isDragging;

        private Point3D? lastDragPosition;

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
            cylinder.Material = material;
            cylinder.BackMaterial = new DiffuseMaterial { Brush = Brushes.Red };
        }

        public override void DragStart(LineRange startRay)
        {
            lastDragPosition = PointOnSketchPlane(startRay);
            isDragging = true;
        }

        public override void Drag(LineRange currRay)
        {
            var currDragPosition = PointOnSketchPlane(currRay);
            var moveVector = currDragPosition - lastDragPosition;

            if (moveVector != null)
                viewModel.Center = viewModel.Center + moveVector.Value;

            if (currDragPosition != null)
                lastDragPosition = currDragPosition;
        }

        public override void DragEnd()
        {
            isDragging = false;
            // TODO: We do nothing here.
        }

        public override bool IsDragging
        {
            get { return isDragging; }
        }

        private Point3D? PointOnSketchPlane(LineRange lineRange)
        {
            var sketchPlane = viewModel.SketchPlane;
            return sketchPlane.PointFromRay(lineRange);
        }
    }
}
