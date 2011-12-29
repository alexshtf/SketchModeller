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
    class NewBGCViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;

        private static readonly ReadOnlyCollection<ComponentViewModel> EMPTY_COMPONENTS =
            Array.AsReadOnly(new ComponentViewModel[0]);

        private NewBendedGenCylinder model;
        private bool isUpdating;

        // the idex of the component that will be edited by the drag operation.
        private int dragStartComponent;

        [InjectionConstructor]
        public NewBGCViewModel(
            UiState uiState = null,
            ICurveAssigner curveAssigner = null,
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            model = new NewBendedGenCylinder();
            components = EMPTY_COMPONENTS;
        }

        public void Init(NewBendedGenCylinder newBgc)
        {
            Model = model = newBgc;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            //MessageBox.Show("InsidePerformDragCore");
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
                    model.Center.Value = value;
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
                    model.Axis.Value = value;
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

        private static BendedCylinderComponent[] GenerateComponents(IEnumerable<ComponentViewModel> cvms)
        {
            var resultQuery =
                from cvm in cvms
                select new BendedCylinderComponent(cvm.Radius, cvm.Progress, cvm.Pnt3D, cvm.Pnt2D);

            return resultQuery.ToArray();
        }

        private static ReadOnlyCollection<ComponentViewModel> GenerateComponentViewModels(IEnumerable<BendedCylinderComponent> ccs)
        {
            var resultQuery =
                from cc in ccs
                select new ComponentViewModel(cc.Radius, cc.Progress, cc.Pnt2D, cc.Pnt3D);

            return Array.AsReadOnly(resultQuery.ToArray());
        }

        #endregion

        #region ComponentViewModel class

        public class ComponentViewModel
        {
            private readonly double radius;
            private readonly double progress;
            private readonly Point pnt2D;
            private readonly Point3D pnt3D;

            public ComponentViewModel(double radius, double progress, Point pnt2D, Point3D pnt3D)
            {
                this.radius = radius;
                this.progress = progress;
                this.pnt2D = pnt2D;
                this.pnt3D = pnt3D;
            }

            public double Radius
            {
                get { return radius; }
            }

            public double Progress
            {
                get { return progress; }
            }
            public Point Pnt2D
            {
                get { return pnt2D; }
            }
            public Point3D Pnt3D
            {
                get { return pnt3D; }
            }
        }

        #endregion

        protected override void PerformDragCore(
           Vector dragVector2d,
           Vector3D dragVector3d,
           Vector3D axisDragVector,
           Point3D? sketchPlanePosition)
        {
            //MessageBox.Show("InsidePerformDragCore");
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                Center = Center + dragVector3d;
                var newComponents =
                    from comp in Components
                    select new ComponentViewModel(comp.Radius, comp.Progress, new Point(comp.Pnt2D.X + dragVector2d.X, comp.Pnt2D.Y + dragVector2d.Y),
                                                  new Point3D(comp.Pnt3D.X + dragVector3d.X, comp.Pnt3D.Y + dragVector3d.Y, comp.Pnt3D.Z + dragVector3d.Z));
                Components = Array.AsReadOnly(newComponents.ToArray());
            }
            else if (Keyboard.Modifiers == AXIS_MOVE_MODIFIER)
                Center = Center + axisDragVector;
            else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
            {
                //MessageBox.Show("InsidePerformDragCore");
                Axis = TrackballRotate(Axis, dragVector2d);
            }
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

        private ReadOnlyCollection<NewBGCViewModel.ComponentViewModel> RecomputeComponents(ReadOnlyCollection<NewBGCViewModel.ComponentViewModel> readOnlyCollection, double radiusDelta, int dragStartComponent)
        {
            throw new NotImplementedException();
        }
    }


}