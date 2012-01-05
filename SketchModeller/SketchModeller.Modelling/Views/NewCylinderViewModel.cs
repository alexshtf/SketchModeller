using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;
using SketchModeller.Utilities;
using Utils;

using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Commands;
using CollectionUtils = Utils.CollectionUtils;
using MathUtils3D = Utils.MathUtils3D;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Services;
using System.Windows.Input;
using System.Windows;
using SketchModeller.Modelling.Editing;
using Petzold.Media3D;

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private NewCylinder model;
        private bool isUpdating;

        [InjectionConstructor]
        public NewCylinderViewModel(UiState uiState = null, ICurveAssigner curveAssigner = null, IEventAggregator eventAggregator = null,
            IConstrainedOptimizer optimizer = null)
            : base(uiState, curveAssigner, eventAggregator, optimizer)
        {
            // set default data
            diameter = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            model = new NewCylinder();
            model.Axis.Value = axis;
            model.Length.Value = length;
            model.Diameter.Value = diameter;
        }

        public void Init(NewCylinder newCylinder)
        {
            Contract.Requires(newCylinder != null);
            Contract.Requires(newCylinder.Axis != MathUtils3D.ZeroVector);
            Contract.Requires(newCylinder.Length > 0);
            Contract.Requires(newCylinder.Diameter > 0);
            
            Model = model = newCylinder;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            isUpdating = true;
            try
            {
                Center = model.Center;
                Axis = model.Axis;
                Length = model.Length;
                Diameter = model.Diameter;
            }
            finally
            {
                isUpdating = false;
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
                if (!isUpdating)
                    model.Axis.Value = value;
            }
        }

        #endregion

        #region Diameter property

        private double diameter;

        public double Diameter
        {
            get { return diameter; }
            set
            {
                diameter = value;
                RaisePropertyChanged(() => Diameter);
                if (!isUpdating)
                    model.Diameter.Value = value;
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
                if (!isUpdating)
                    model.Center.Value = value;
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
                if (!isUpdating)
                    model.Length.Value = value;
            }
        }

        #endregion

        #region Editor class

        private class Editor : BaseEditor
        {
            private NewCylinderViewModel viewModel;

            public Editor(Point startPos, LineRange lineRange, NewCylinderViewModel viewModel)
                : base(startPos, lineRange, viewModel)
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
                        var diameterDelta = Vector3D.DotProduct(axis, vector3D);
                        viewModel.Diameter = Math.Max(NewCylinderViewModel.MIN_DIAMETER, viewModel.Diameter + diameterDelta);
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


        public override IEditor StartEdit(Point startPos, LineRange startRay)
        {
            return new Editor(startPos, startRay, this);
        }

        public override Vector3D ApproximateAxis
        {
            get { return Axis; }
        }
    }
}
