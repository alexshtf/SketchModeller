using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Commands;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Services;
using System.Windows;
using System.Windows.Input;
using SketchModeller.Modelling.Editing;
using Petzold.Media3D;

namespace SketchModeller.Modelling.Views
{
    public class NewConeViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private NewCone model;
        private bool initializing;

        [InjectionConstructor]
        public NewConeViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null,
            IConstrainedOptimizer optimizer = null)
            : base(uiState, curveAssigner, eventAggregator, optimizer)
        {
            topRadius = 0.2;
            bottomRadius = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            center = MathUtils3D.Origin;

            model = new NewCone();
            UpdateModel();
        }

        public void Init(NewCone newModel)
        {
            this.model = newModel;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            initializing = true;
            try
            {
                Axis = model.Axis;
                Center = model.Center;
                Length = model.Length;
                TopRadius = model.TopRadius;
                BottomRadius = model.BottomRadius;
            }
            finally
            {
                initializing = false;
            }
        }

        #region Axis property

        private Vector3D axis;

        public Vector3D Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                RaisePropertyChanged(() => Axis);
            }
        }

        #endregion

        #region Center property

        private Point3D center;

        public Point3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
            }
        }

        #endregion

        #region Length property

        private double length;

        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
            }
        }

        #endregion

        #region TopRadius property

        private double topRadius;

        public double TopRadius
        {
            get { return topRadius; }
            set
            {
                topRadius = value;
                RaisePropertyChanged(() => TopRadius);
            }
        }

        #endregion

        #region BottomRadius property

        private double bottomRadius;

        public double BottomRadius
        {
            get { return bottomRadius; }
            set
            {
                bottomRadius = value;
                RaisePropertyChanged(() => BottomRadius);
            }
        }

        #endregion

        public DragStartProximities DragStartProximity { get; set; }

        public override IEditor StartEdit(Point startPos, LineRange startRay)
        {
            return new Editor(startPos, startRay, this);
        }

        public override Vector3D ApproximateAxis
        {
            get { return Axis; }
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            UpdateModel();
        }

        private void UpdateModel()
        {
            if (!initializing)
            {
                model.Axis.Value = axis;
                model.Center.Value = center;
                model.Length.Value = length;
                model.TopRadius.Value = topRadius;
                model.BottomRadius.Value = bottomRadius;
            }
        }

        public enum DragStartProximities
        {
            Top,
            Bottom,
        }

        #region Editor class

        private class Editor : BaseEditor
        {
            private NewConeViewModel viewModel;

            public Editor(Point startPoint, LineRange startRay, NewConeViewModel viewModel)
                : base(startPoint, startRay, viewModel)
            {
                this.viewModel = viewModel;
            }

            protected override void PerformDrag(Vector dragVector2d, Vector3D vector3D, Vector3D axisDragVector, Point3D? currDragPosition)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                    viewModel.Center = viewModel.Center + vector3D;
                else if (Keyboard.Modifiers == AXIS_MOVE_MODIFIER)
                    viewModel.Center = viewModel.Center + axisDragVector;
                else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
                {
                    viewModel.Axis = viewModel.TrackballRotate(viewModel.Axis, dragVector2d);
                }
                else if (Keyboard.Modifiers == DIAMETER_MODIFIER)
                {
                    var axis = Vector3D.CrossProduct(viewModel.Axis, viewModel.SketchPlane.Normal);
                    if (axis != default(Vector3D))
                    {
                        axis.Normalize();
                        var radiusDelta = 0.5 * Vector3D.DotProduct(axis, vector3D);
                        if (viewModel.DragStartProximity == DragStartProximities.Top)
                            viewModel.TopRadius = Math.Max(NewConeViewModel.MIN_DIAMETER, viewModel.TopRadius + radiusDelta);
                        else if (viewModel.DragStartProximity == DragStartProximities.Bottom)
                            viewModel.BottomRadius = Math.Max(NewConeViewModel.MIN_DIAMETER, viewModel.BottomRadius + radiusDelta);
                    }
                }
                else if (Keyboard.Modifiers == LENGTH_MODIFIER)
                {
                    var axis = viewModel.Axis.Normalized();
                    var lengthDelta = Vector3D.DotProduct(axis, vector3D) * 2;
                    viewModel.Length = Math.Max(NewCylinderViewModel.MIN_LENGTH, viewModel.Length + lengthDelta);
                }
            }
        }


        #endregion
    }
}
