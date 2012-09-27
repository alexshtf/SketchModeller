using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Services.Assign
{
    class CurveAssigner : ICurveAssigner
    {
        private readonly SessionData sessionData;

        public CurveAssigner(SessionData sessionData)
        {
            this.sessionData = sessionData;
        }
        public void Refresh(NewPrimitive primitive)
        {
            for (int idx = 0; idx < sessionData.SketchObjects.Length; idx++)
                sessionData.SketchObjects[idx].isdeselected = false;
            for (int idx = 0; idx < primitive.AllCurves.Length; idx++)
            {
                primitive.AllCurves[idx].isDeselected = false;
                if (primitive.AllCurves[idx].AssignedTo != null)
                    primitive.AllCurves[idx].AssignedTo.isdeselected = false;
            }
        }

        public bool ComputeSilhouetteAssignments(NewPrimitive primitive)
        {
            ComputeAssignments(primitive.SilhouetteCurves, CurveCategories.Silhouette);
            int toDoSilhouetteCurves = primitive.SilhouetteCurves.Where(curve => !curve.isDeselected).ToArray().Length;
            if (toDoSilhouetteCurves == 0) return false;
            int toDoSilhouettesOnSketch =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == CurveCategories.Silhouette
                 where sessionData.SketchObjects[idx].isdeselected == false
                 select idx).ToArray().Length;
            if (toDoSilhouettesOnSketch == 0) return false;
            else return true;
        }

        public bool ComputeFeatureAssignments(PrimitiveCurve[] features)
        {
            ComputeAssignments(features, CurveCategories.Feature);
            int toDoFeatureCurves = features.Where(curve => !curve.isDeselected).ToArray().Length;
            if (toDoFeatureCurves == 0) return false;
            int toDoFeaturesOnSketch =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == CurveCategories.Feature
                 where sessionData.SketchObjects[idx].isdeselected == false
                 select idx).ToArray().Length;
            if (toDoFeaturesOnSketch == 0) return false;
            else return true;
        }

        public bool ComputeFeatureAssignments(NewPrimitive primitive)
        {
            ComputeAssignments(primitive.FeatureCurves, CurveCategories.Feature);
            int toDoFeatureCurves = primitive.FeatureCurves.Where(curve => !curve.isDeselected).ToArray().Length;
            if (toDoFeatureCurves == 0) return false;
            int toDoFeaturesOnSketch =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == CurveCategories.Feature
                 where sessionData.SketchObjects[idx].isdeselected == false
                 select idx).ToArray().Length;
            if (toDoFeaturesOnSketch == 0) return false;
            else return true;
        }

        public void ComputeAssignments(NewPrimitive primitive, bool refresh)
        {
            ComputeAssignments(primitive.FeatureCurves, CurveCategories.Feature);
            //if (!onlyFeatures)
            ComputeAssignments(primitive.SilhouetteCurves, CurveCategories.Silhouette);
            //from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
            if (refresh)
                for (int idx = 0; idx < sessionData.SketchObjects.Length; idx++)
                    sessionData.SketchObjects[idx].isdeselected = false;
        }

        private void ComputeAssignments(PrimitiveCurve[] curves, CurveCategories category)
        {
            curves = curves.Where(curve => !curve.isDeselected).ToArray();
            if (curves.Length == 0)
                return;

            // get distance transforms of sketch curves according to the given category
            var distanceTransforms =
                (from idx in Enumerable.Range(0, sessionData.SketchObjects.Length)
                 where sessionData.SketchObjects[idx].CurveCategory == category
                 where sessionData.SketchObjects[idx].isdeselected == false
                 select new { Index = idx, DistanceTransform = sessionData.DistanceTransforms[idx] }
                ).ToArray();

            //System.Diagnostics.Debug.WriteLine("Number:" + distanceTransforms.Length);

            // compute matching costs using distance transform integral
            if (distanceTransforms.Length > 0)
            {
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
                int[] assignments = new int[curves.Length];

                if (distanceTransforms.Length > 1)
                    assignments = HungarianAlgorithm.FindMaxWeightAssignments(matchingMatrix);
                else
                {
                    if (curves.Length > 1)
                        if (matchingMatrix[0, 0] > matchingMatrix[1, 0])
                        {
                            assignments[0] = 0;
                            assignments[1] = -1;
                        }
                        else
                        {
                            assignments[1] = 0;
                            assignments[0] = -1;
                        }
                    else assignments[0] = 0;
                }
                // assign object curves to sketch curves according to the computed assignments
                for (int i = 0; i < assignments.Length; ++i)
                {
                    var assignedTo = assignments[i];
                    if (assignedTo >= 0)
                    {
                        var assignedToCurve = sessionData.SketchObjects[distanceTransforms[assignedTo].Index];
                        curves[i].AssignedTo = assignedToCurve;
                        curves[i].ClosestPoint =
                            DistanceTransformIntegral.MinDistancePoint(
                        curves[i].Points,
                        distanceTransforms[assignedTo].DistanceTransform);
                    }
                    else
                    {
                        curves[i].AssignedTo = null;
                    }
                }
            }
        }
    }
}
