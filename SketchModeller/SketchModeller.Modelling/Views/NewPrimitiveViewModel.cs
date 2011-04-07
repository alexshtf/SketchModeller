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

namespace SketchModeller.Modelling.Views
{
    abstract public class NewPrimitiveViewModel : NotificationObject
    {
        protected UiState uiState;
        protected SessionData sessionData;

        public NewPrimitiveViewModel(UiState uiState = null, SessionData sessionData = null)
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
            this.uiState = uiState;
            this.sessionData = sessionData;
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }
        public NewPrimitive Model { get; set; }
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }

        public abstract void UpdateFromModel();

        public void SelectCandidateCurves(IEnumerable<Point[]> featureCurves)
        {
            var featureCurvesArray = featureCurves.ToArray();
            int[,] matchCosts = new int[featureCurvesArray.Length, sessionData.DistanceTransforms.Length];

            // compute matching costs using distance transform integral and put them in matchCosts
            for (int i = 0; i < featureCurvesArray.Length; ++i)
            {
                for (int j = 0; j < sessionData.DistanceTransforms.Length; ++j)
                {
                    var integral = DistanceTransformIntegral.Compute(featureCurvesArray[i], sessionData.DistanceTransforms[j]);
                    matchCosts[i, j] = (int)Math.Round(integral);
                }
            }

            // find best assignments of feature curves to sketch curves
            var assignments = HungarianAlgorithm.FindAssignments(matchCosts);

            // unselect all curves
            var selectedCurves = sessionData.SelectedSketchObjects.ToArray();
            foreach(var sc in selectedCurves)
                sc.IsSelected = false;

            // select the relevant sketch curves
            foreach (var assignment in assignments)
                sessionData.SketchObjects[assignment].IsSelected = true;
        }
    }
}
