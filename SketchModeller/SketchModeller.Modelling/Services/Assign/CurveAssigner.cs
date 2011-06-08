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

        public void ComputeAssignments(NewPrimitive primitive)
        {
            ComputeAssignments(primitive.FeatureCurves, CurveCategories.Feature);
            ComputeAssignments(primitive.SilhouetteCurves, CurveCategories.Silhouette);
        }

        private void ComputeAssignments(PrimitiveCurve[] curves, CurveCategories category)
        {
            curves = curves.Where(curve => !curve.IsUserAssignment).ToArray();

            if (curves.Length == 0)
                return;

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
            var assignments = HungarianAlgorithm.FindMaxWeightAssignments(matchingMatrix);

            // assign object curves to sketch curves according to the computed assignments
            for (int i = 0; i < assignments.Length; ++i)
            {
                var assignedTo = assignments[i];
                var assignedToCurve = sessionData.SketchObjects[distanceTransforms[assignedTo].Index];

                curves[i].AssignedTo = assignedToCurve;
                curves[i].ClosestPoint =
                    DistanceTransformIntegral.MinDistancePoint(
                        curves[i].Points,
                        distanceTransforms[assignedTo].DistanceTransform);
            }
        }
    }
}
