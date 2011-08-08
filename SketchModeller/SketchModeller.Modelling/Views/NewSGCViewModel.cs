using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Utils;

namespace SketchModeller.Modelling.Views
{
    class NewSGCViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private static readonly ReadOnlyCollection<ComponentViewModel> EMPTY_COMPONENTS =
            Array.AsReadOnly(new ComponentViewModel[0]);

        private NewStraightGenCylinder model;
        private bool isUpdating;

        // the idex of the component that will be edited by the drag operation.
        private int dragStartComponent; 

        [InjectionConstructor]
        public NewSGCViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            model = new NewStraightGenCylinder();
            components = EMPTY_COMPONENTS;
        }

        public void Init(NewStraightGenCylinder newSgc)
        {
            Model = model = newSgc;
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
                Components = GenerateComponentViewModels(model.Components);
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
                    model.Center = value;
            }
        }

        #endregion

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
                    model.Axis = value;
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
                    model.Length = value;
            }
        }

        #endregion

        #region Components property

        private ReadOnlyCollection<ComponentViewModel> components;

        public ReadOnlyCollection<ComponentViewModel> Components
        {
            get { return components; }
            set
            {
                components = value;
                RaisePropertyChanged(() => Components);
                if (!isUpdating)
                    model.Components = GenerateComponents(value);
            }
        }

        #endregion
        
        #region component <--> component view models methods

        private static CylinderComponent[] GenerateComponents(IEnumerable<ComponentViewModel> cvms)
        {
            var resultQuery =
                from cvm in cvms
                select new CylinderComponent(cvm.Radius, cvm.Progress);

            return resultQuery.ToArray();
        }

        private static  ReadOnlyCollection<ComponentViewModel> GenerateComponentViewModels(IEnumerable<CylinderComponent> ccs)
        {
            var resultQuery =
                from cc in ccs
                select new ComponentViewModel(cc.Radius, cc.Progress);

            return Array.AsReadOnly(resultQuery.ToArray());
        }

        #endregion

        #region ComponentViewModel class

        public class ComponentViewModel
        {
            private readonly double radius;
            private readonly double progress;

            public ComponentViewModel(double radius, double progress)
            {
                this.radius = radius;
                this.progress = progress;
            }

            public double Radius
            {
                get { return radius; }
            }

            public double Progress
            {
                get { return progress; }
            }
        }

        #endregion

        protected override void PerformDragCore(
           Vector dragVector2d,
           Vector3D dragVector3d,
           Vector3D axisDragVector,
           Point3D? sketchPlanePosition)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
                Center = Center + dragVector3d;
            else if (Keyboard.Modifiers == AXIS_MOVE_MODIFIER)
                Center = Center + axisDragVector;
            else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
                Axis = TrackballRotate(Axis, dragVector2d);
            else if (Keyboard.Modifiers == DIAMETER_MODIFIER)
            {
                var axis = Vector3D.CrossProduct(Axis, SketchPlane.Normal);
                if (axis != default(Vector3D))
                {
                    axis.Normalize();
                    var radiusDelta = 0.5 * Vector3D.DotProduct(axis, dragVector3d);
                    Components = RecomputeComponents(
                        Components,
                        radiusDelta,
                        dragStartComponent);
                }
            }
            else if (Keyboard.Modifiers == LENGTH_MODIFIER)
            {
                var axis = Axis.Normalized();
                var lengthDelta = Vector3D.DotProduct(axis, dragVector3d) * 2;
                Length = Math.Max(MIN_LENGTH, Length + lengthDelta);
            }
        }

        private ReadOnlyCollection<NewSGCViewModel.ComponentViewModel> RecomputeComponents(ReadOnlyCollection<NewSGCViewModel.ComponentViewModel> readOnlyCollection, double radiusDelta, int dragStartComponent)
        {
            throw new NotImplementedException();
        }
    }


}
