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

namespace SketchModeller.Modelling.Views
{
    abstract public class NewPrimitiveViewModel : NotificationObject
    {
        protected UiState uiState;
        protected SessionData sessionData;
        protected IEventAggregator eventAggregator;

        public NewPrimitiveViewModel(UiState uiState, SessionData sessionData, IEventAggregator eventAggregator)
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
            this.uiState = uiState;
            this.sessionData = sessionData;
            this.eventAggregator = eventAggregator;
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }
        public NewPrimitive Model { get; set; }
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }

        public abstract void UpdateFromModel();

        public void NotifyDragged()
        {
            Model.UpdateCurvesGeometry();
            ComputeCurvesAssignment();
            eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Publish(Model);
        }

        private void ComputeCurvesAssignment()
        {
            ComputeAssignments(Model.FeatureCurves, CurveCategories.Feature);
            ComputeAssignments(Model.SilhouetteCurves, CurveCategories.Silhouette);
        }

        private void ComputeAssignments(PrimitiveCurve[] curves, CurveCategories category)
        {
            // get distance transforms of sketch curves according to the given category
            var distanceTransforms =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == category
                 select new { Index = idx, DistanceTransform = sessionData.DistanceTransforms[idx] }
                ).ToArray();


            // compute matching costs using distance transform integral
            int[,] matchingMatrix = new int[curves.Length, distanceTransforms.Length];
            var matchingMatrixIndices =
                from i in Enumerable.Range(0, curves.Length)
                from j in Enumerable.Range(0, distanceTransforms.Length)
                select new { I = i, J = j };
            Parallel.ForEach(matchingMatrixIndices, item =>
                {
                    var i = item.I;
                    var j = item.J;
                    var integral = DistanceTransformIntegral.Compute(curves[i].Points, distanceTransforms[j].DistanceTransform);
                    matchingMatrix[i, j] = (int)Math.Round(integral);
                });

            // compute minimum-cost assignments of primitive curves to sketch curves
            var assignments = HungarianAlgorithm.FindAssignments(matchingMatrix);

            // assign object curves to sketch curves according to the computed assignments
            for (int i = 0; i < assignments.Length; ++i)
            {
                var assignedTo = assignments[i];
                curves[i].AssignedTo =
                    sessionData.SketchObjects[distanceTransforms[assignedTo].Index];
                curves[i].ClosestPoint =
                    DistanceTransformIntegral.MinDistancePoint(
                        curves[i].Points, 
                        distanceTransforms[assignedTo].DistanceTransform);
            }
        }

    }
}
