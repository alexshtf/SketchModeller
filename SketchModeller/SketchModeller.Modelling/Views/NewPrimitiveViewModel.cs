using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure;
using System.Collections.ObjectModel;
using SketchModeller.Infrastructure.Data;

using UiState = SketchModeller.Infrastructure.Shared.UiState;
using SketchPlane = SketchModeller.Infrastructure.Data.SketchPlane;
using System.Windows;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Utilities;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Modelling.Events;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Services;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Views
{
    abstract public class NewPrimitiveViewModel : NotificationObject, IWeakEventListener
    {
        private NewPrimitive model;
        protected UiState uiState;
        protected IEventAggregator eventAggregator;
        protected ICurveAssigner curveAssigner;

        public NewPrimitiveViewModel(UiState uiState, ICurveAssigner curveAssigner, IEventAggregator eventAggregator)
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
            this.uiState = uiState;
            this.curveAssigner = curveAssigner;
            this.eventAggregator = eventAggregator;
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }

        public NewPrimitive Model 
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    model.RemoveListener(this, () => model.IsSelected);
                    if (model.IsSelected)
                        model.ClearColorCodingFromSketch();
                }

                model = value;

                if (model != null)
                {
                    model.AddListener(this, () => model.IsSelected);
                    if (model.IsSelected)
                        model.SetColorCodingToSketch();
                }
            }
        }
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }

        public abstract void UpdateFromModel();

        protected abstract void PerformDragCore(Vector dragVector2d, Vector3D dragVector3d, Vector3D axisDragVector, Point3D? sketchPlanePosition);

        public void PerformDrag(Vector dragVector2d, Vector3D dragVector3d, Vector3D axisDragVector, Point3D? sketchPlanePosition)
        {
            PerformDragCore(dragVector2d, dragVector3d, axisDragVector, sketchPlanePosition);
            NotifyDragged();
        }

        public void NotifyDragged()
        {
            ApplyConstraints();
            UpdateFromModel();
            Model.UpdateCurvesGeometry();
            ComputeCurvesAssignment();
            eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Publish(Model);
        }

        private void ApplyConstraints()
        {
            if (model.EditConstraints != null)
            {
                var projector = new ConstraintsProjector(model.EditConstraints);
                projector.Project();
            }
        }

        protected Vector3D TrackballRotate(Vector3D toRotate, Vector dragVector2d)
        {
            const double TRACKBALL_ROTATE_SPEED = 0.5;

            var horzDegrees = -dragVector2d.X * TRACKBALL_ROTATE_SPEED;
            var vertDegrees = -dragVector2d.Y * TRACKBALL_ROTATE_SPEED;

            var horzAxis = SketchPlane.Normal;
            var vertAxis = SketchPlane.XAxis;

            toRotate = RotationHelper.RotateVector(toRotate, horzAxis, horzDegrees);
            toRotate = RotationHelper.RotateVector(toRotate, vertAxis, vertDegrees);
            return toRotate;
        }

        private void ComputeCurvesAssignment()
        {
            if (Model.IsSelected)
                Model.ClearColorCodingFromSketch();

            curveAssigner.ComputeAssignments(Model);
            
            if (Model.IsSelected)
                Model.SetColorCodingToSketch();
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            if (Model.IsSelected)
                Model.SetColorCodingToSketch();
            else
                Model.ClearColorCodingFromSketch();

            return true;
        }
    }
}
