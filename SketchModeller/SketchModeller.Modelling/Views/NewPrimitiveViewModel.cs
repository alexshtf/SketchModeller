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
using Petzold.Media3D;
using SketchModeller.Modelling.Editing;
using System.Diagnostics;

namespace SketchModeller.Modelling.Views
{
    abstract public class NewPrimitiveViewModel : NotificationObject, IWeakEventListener, IEditable
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

        /// <summary>
        /// Updates the intrinsic properties of this view model from the data model. Called when we know
        /// that the model has changes, and the view-model should reflect those changes.
        /// </summary>
        public abstract void UpdateFromModel();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="startRay"></param>
        /// <returns></returns>
        public abstract IEditor StartEdit(Point startPos, LineRange startRay);

        #region IEditable implementation 

        public abstract Vector3D ApproximateAxis { get; }
        
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }
        
        public void NotifyDragged()
        {
            ApplyConstraints();
            UpdateFromModel();
            Model.UpdateCurvesGeometry();
            ComputeCurvesAssignment();
            eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Publish(Model);
        }
        
        #endregion

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
            bool refresh = true;
            //Compute at least one silhouette curve
            curveAssigner.ComputeSilhouetteAssignments(Model);
            //Debug.WriteLine("Active Silhouettes Before:" + ActiveSilhouettes);
            //Pick the first silhouette Curve
            Model.SilhouetteCurves[0].isDeselected = true;
            Model.SilhouetteCurves[0].AssignedTo.isdeselected = true;
            //Find all compatible feature curve
            while (curveAssigner.ComputeFeatureAssignments(Model))
                Model.CheckFeatureCurves();
            var NumberOfActiveFeatureCurves = Model.FeatureCurves
                .Select(c => c)
                .Where(c => c.AssignedTo != null)
                .ToArray().Length;
            if (NumberOfActiveFeatureCurves > 0)
                while (curveAssigner.ComputeSilhouetteAssignments(Model))
                {
                    if (NumberOfActiveFeatureCurves == 1) Model.CheckSilhoueteCurveWith1Feature();
                    else Model.CheckSilhoueteCurveWith2Features();
                }
         
         
            curveAssigner.refresh(Model);
            Model.CanSnap = false;
            Model.CanSnap = CheckIfPrimitiveCanSnap(); 
            
            if (Model.IsSelected)
                Model.SetColorCodingToSketch();
        }

        public bool CheckIfPrimitiveCanSnap()
        {
            bool ModelCanSnap = false;
            if (Model.GetType() == typeof(NewSphere)) {
                if (Model.SilhouetteCurves.Length > 0)
                    ModelCanSnap = true;
            }
            if (Model.GetType() == typeof(NewCylinder))
            {
                if (Model.SilhouetteCurves.Length > 1)
                    if (Model.SilhouetteCurves[0].AssignedTo != null && Model.SilhouetteCurves[1].AssignedTo != null) 
                        ModelCanSnap = true;
            }
            if (Model.GetType() == typeof(NewCone))
            {
                if (Model.SilhouetteCurves.Length > 1)
                    if (Model.SilhouetteCurves[0].AssignedTo != null && Model.SilhouetteCurves[1].AssignedTo != null)
                        ModelCanSnap = true;
            }
            if (Model.GetType() == typeof(NewStraightGenCylinder))
            {
                if (Model.SilhouetteCurves.Length > 1)
                    if (Model.SilhouetteCurves[0].AssignedTo != null && Model.SilhouetteCurves[1].AssignedTo != null)
                        ModelCanSnap = true;
            }
            return ModelCanSnap;
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
