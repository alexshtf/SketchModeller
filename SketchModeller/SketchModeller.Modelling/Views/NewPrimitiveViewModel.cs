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

        public void SelectCandidateCurves(IEnumerable<Point[]> featureCurves, IEnumerable<Point[]> silhouetteCurves)
        {
            var featureCurveInfos = GetDistanceTransformsByCategory(CurveCategories.Feature);
            var silhouetteCurveInfos = GetDistanceTransformsByCategory(CurveCategories.Silhouette);
            
            var featuresToSelect = SelectSpecificCurves(featureCurves, featureCurveInfos);
            var silhouettesToSelect = SelectSpecificCurves(silhouetteCurves, silhouetteCurveInfos);

            var selectedCurves = sessionData.SelectedSketchObjects.ToArray();
            foreach (var sc in selectedCurves)
                sc.IsSelected = false;

            foreach (var idx in featuresToSelect.Concat(silhouettesToSelect))
                sessionData.SketchObjects[idx].IsSelected = true;
        }

        private Tuple<int, int[,]>[] GetDistanceTransformsByCategory(CurveCategories category)
        {
            var result =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == category
                 select Tuple.Create(idx, sessionData.DistanceTransforms[idx])
                ).ToArray();
            return result;
        }

        private int[] SelectSpecificCurves(IEnumerable<Point[]> curves, Tuple<int, int[,]>[] curveInfos)
        {
            var featureCurvesArray = curves.ToArray();
            int[,] matchCosts = new int[featureCurvesArray.Length, curveInfos.Length];

            // compute matching costs using distance transform integral and put them in matchCosts
            for (int i = 0; i < featureCurvesArray.Length; ++i)
            {
                for (int j = 0; j < curveInfos.Length; ++j)
                {
                    var integral =
                        DistanceTransformIntegral.Compute(featureCurvesArray[i], curveInfos[j].Item2);
                    matchCosts[i, j] = (int)Math.Round(integral);
                }
            }

            // find best assignments of feature curves to sketch curves
            var assignments = HungarianAlgorithm.FindAssignments(matchCosts);

            var result = (from assignment in assignments
                          select curveInfos[assignment].Item1
                         ).ToArray();
            return result;
        }
    }
}
