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
    public class NewCuboidViewModel : NewPrimitiveViewModel
    {
        public const double MIN_WIDTH = 0.01;
        public const double MIN_HEIGHT = 0.01;
        public const double MIN_DEPTH = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys WIDTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys HEIGHT_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys DEPTH_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private NewCuboid model;
        private bool isUpdating;

        [InjectionConstructor]
        public NewCuboidViewModel(UiState uiState = null, ICurveAssigner curveAssigner = null, IEventAggregator eventAggregator = null,
            IConstrainedOptimizer optimizer = null)
            : base(uiState, curveAssigner, eventAggregator, optimizer)
        {
            // set default data
            /*diameter = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;*/
            model = new NewCuboid();
            /*model.Axis.Value = axis;
            model.Length.Value = length;
            model.Diameter.Value = diameter;*/
        }

        public void Init(NewCuboid newCuboid)
        {
            Model = model = newCuboid;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            isUpdating = true;
            try
            {
                Center = model.Center;
                H = model.H;
                W = model.W;
                D = model.D;
                Width = model.Width;
                Height = model.Height;
                Depth = model.Depth;
            }
            finally
            {
                isUpdating = false;
            }
        }

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

        #region Width property

        private double width;

        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged(() => Width);
                if (!isUpdating)
                    model.Width.Value = value;
            }
        }

        #endregion

        #region Height property

        private double height;

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged(() => Height);
                if (!isUpdating)
                    model.Height.Value = value;
            }
        }

        #endregion

        #region Depth property

        private double depth;

        public double Depth
        {
            get { return depth; }
            set
            {
                depth = value;
                RaisePropertyChanged(() => Depth);
                if (!isUpdating)
                    model.Depth.Value = value;
            }
        }

        #endregion

        #region VectorWidth property

        private Vector3D w;

        public Vector3D W
        {
            get { return w; }
            set
            {
                w = value;
                RaisePropertyChanged(() => W);
                if (!isUpdating)
                    model.W.Value = value;
            }
        }

        #endregion

        #region VectorHeight property

        private Vector3D h;

        public Vector3D H
        {
            get { return h; }
            set
            {
                h = value;
                RaisePropertyChanged(() => H);
                if (!isUpdating)
                    model.H.Value = value;
            }
        }

        #endregion

        #region VectorDepth property

        private Vector3D d;

        public Vector3D D
        {
            get { return d; }
            set
            {
                d = value;
                RaisePropertyChanged(() => D);
                if (!isUpdating)
                    model.D.Value = value;
            }
        }

        #endregion

        #region Editor class

        private class Editor : BaseEditor
        {
            private NewCuboidViewModel viewModel;

            public Editor(Point startPos, LineRange lineRange, NewCuboidViewModel viewModel)
                : base(startPos, lineRange, viewModel)
            {
                this.viewModel = viewModel;
            }

            protected override void PerformDrag(Vector dragVector2d, Vector3D vector3D, Vector3D axisDragVector, Point3D? currDragPosition)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                    viewModel.Center = viewModel.Center + vector3D;
                else if (Keyboard.Modifiers == DEPTH_MODIFIER)
                {
                    var diameterDelta = Vector3D.DotProduct(new Vector3D(0, 1, 0), vector3D);
                    viewModel.Depth = Math.Max(NewCuboidViewModel.MIN_DEPTH, viewModel.Depth + diameterDelta);
                }
                else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
                {
                    viewModel.W = viewModel.TrackballRotate(viewModel.W, dragVector2d);
                    viewModel.H = viewModel.TrackballRotate(viewModel.H, dragVector2d);
                    viewModel.D = viewModel.TrackballRotate(viewModel.D, dragVector2d);
                    viewModel.W.Normalized();
                    viewModel.H.Normalized();
                    viewModel.D.Normalized();
                }
                else if (Keyboard.Modifiers == WIDTH_MODIFIER)
                {
                    var diameterDelta = Vector3D.DotProduct(new Vector3D(1, 0, 0), vector3D);
                    viewModel.Width = Math.Max(NewCuboidViewModel.MIN_WIDTH, viewModel.Width + diameterDelta);
                }
                else if (Keyboard.Modifiers == HEIGHT_MODIFIER)
                {
                    var diameterDelta = Vector3D.DotProduct(new Vector3D(0, 1, 0), vector3D);
                    viewModel.Height = Math.Max(NewCuboidViewModel.MIN_HEIGHT, viewModel.Height + diameterDelta);
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
            get { return new Vector3D(0, 0, 0); }
        }
    }
}
