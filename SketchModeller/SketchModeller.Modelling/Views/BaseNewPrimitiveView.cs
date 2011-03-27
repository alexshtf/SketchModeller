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

        private bool isDragging;
        private Point3D? lastDragPosition3d;
        private Point lastDragPosition2d; 

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

        public virtual void DragStart(Point startPos, LineRange startRay)
        {
            lastDragPosition3d = PointOnSketchPlane(startRay);
            lastDragPosition2d = startPos;
            isDragging = true;
        }

        public virtual void Drag(Point currPos, LineRange currRay)
        {
            var currDragPosition = PointOnSketchPlane(currRay);
            var dragVector3d = currDragPosition - lastDragPosition3d;
            var dragVector2d = currPos - lastDragPosition2d;

            if (dragVector3d != null)
            {
                var axisDragVector = MathUtils3D.ProjectVector(dragVector3d.Value, ApproximateAxis);
                PerformDrag(dragVector2d, dragVector3d.Value, axisDragVector);
            }

            if (currDragPosition != null)
                lastDragPosition3d = currDragPosition;
            lastDragPosition2d = currPos;
        }

        public virtual void DragEnd()
        {
            isDragging = false;
        }

        public bool IsDragging
        {
            get { return isDragging; }
        }

        protected abstract void PerformDrag(Vector dragVector2d, Vector3D dragVector3d, Vector3D axisDragVector);
        protected abstract Vector3D ApproximateAxis { get; }

        protected void SetDefaultMaterial(ModelVisualBase mv3d, NewPrimitiveViewModel viewModel)
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

            var backMaterial = new DiffuseMaterial { Brush = Brushes.Red };
            backMaterial.Freeze();

            mv3d.Material = material;
            mv3d.BackMaterial = backMaterial;
        }

        protected Vector3D TrackballRotate(Vector3D toRotate, Vector dragVector2d)
        {
            const double TRACKBALL_ROTATE_SPEED = 0.5;

            var horzDegrees = -dragVector2d.X * TRACKBALL_ROTATE_SPEED;
            var vertDegrees = -dragVector2d.Y * TRACKBALL_ROTATE_SPEED;

            var horzAxis = viewModel.SketchPlane.Normal;
            var vertAxis = viewModel.SketchPlane.XAxis;

            toRotate = RotationHelper.RotateVector(toRotate, horzAxis, horzDegrees);
            toRotate = RotationHelper.RotateVector(toRotate, vertAxis, vertDegrees);
            return toRotate;
        }

        protected Point3D? PointOnSketchPlane(LineRange lineRange)
        {
            var sketchPlane = viewModel.SketchPlane;
            return sketchPlane.PointFromRay(lineRange);
        }
    }
}
