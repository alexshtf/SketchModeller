using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Input;
using SketchModeller.Modelling.Editing;
using Petzold.Media3D;

namespace SketchModeller.Modelling.Views
{
    class NewSphereViewModel : NewPrimitiveViewModel
    {
        private const ModifierKeys RADIUS_MODIFIER = ModifierKeys.Shift;

        public const double MIN_RADIUS = 0.005;

        private NewSphere model;
        private bool isUpdating;

        public NewSphereViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            radius = 0.1;
            model = new NewSphere();
            model.Radius.Value = radius;
        }

        public void Init(NewSphere newSphere)
        {
            Contract.Requires(newSphere != null);
            Contract.Requires(newSphere.Radius > 0);

            Model = model = newSphere;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            isUpdating = true;
            try
            {
                Center = model.Center;
                Radius = model.Radius;
            }
            finally
            {
                isUpdating = false;
            }
        }

        #region Radius property

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RaisePropertyChanged(() => Radius);
                if (!isUpdating)
                    model.Radius.Value = value;
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

        public override IEditor StartEdit(Point startPos, LineRange startRay)
        {
            return new Editor(startPos, startRay, this);
        }

        public override Vector3D ApproximateAxis
        {
            get { return new Vector3D(0, 0, 0); }
        }

        #region Editor class

        class Editor : BaseEditor
        {
            private NewSphereViewModel viewModel;

            public Editor(Point startPoint, LineRange startRay, NewSphereViewModel viewModel)
                : base(startPoint, startRay, viewModel)
            {
                this.viewModel = viewModel;
            }

            protected override void PerformDrag(Vector dragVector2d, Vector3D vector3D, Vector3D axisDragVector, Point3D? currDragPosition)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                    viewModel.Center = viewModel.Center + vector3D;
                if (Keyboard.Modifiers == RADIUS_MODIFIER)
                {
                    if (currDragPosition != null)
                    {
                        var fromCenter = currDragPosition.Value - viewModel.Center;
                        fromCenter.Normalize();
                        var radiusDelta = Vector3D.DotProduct(fromCenter, vector3D);
                        viewModel.Radius = Math.Max(viewModel.Radius + radiusDelta, NewSphereViewModel.MIN_RADIUS);
                    }
                }
            }
        }

        #endregion

    }
}
