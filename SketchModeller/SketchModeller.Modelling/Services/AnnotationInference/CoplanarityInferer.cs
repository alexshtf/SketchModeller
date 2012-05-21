using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using Utils;
using Enumerable = System.Linq.Enumerable;
using UtilsEnumerable = Utils.Enumerable;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    /// <summary>
    /// Infers coplanarity constraints based on a simple heuristic of "almost coplanarity".
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two feature curves will be considered coplanar if:
    /// </para>
    /// <list type="bullet">
    /// <item>The angle between their normal vectors is lower then the specified angle threshold</item>
    /// <item>The distance between the center of the first feature curve and the plane of the second feature curve is less than the distance threshold</item>
    /// <item>The distance between the center of the second feature curve and the plane of the first feature curve is less than the distance threshold</item>
    /// </list>
    /// </remarks>
    class CoplanarityInferer : IInferrer
    {
        public const double DEFAULT_PARALLEL_ANGLE_THRESHOLD = 15 * Math.PI / 180; // 15 degrees
        public const double DEFAULT_CENTER_DISTANCE_THRESHOLD = 0.05;
        public const double DEFAUTL_CENTER_DISTANCE_RELATIVE_THRESHOLD = 0.05; // 10%

        private readonly SessionData sessionData;
        private readonly double parallelAngleThreshold; 
        private readonly double centerDistanceThreshold;
        private readonly double centerDistanceRelativeThreshold;

        /// <summary>
        /// Constructs a new instance of <see cref="CoplanarityInferer"/> class.
        /// </summary>
        /// <param name="sessionData">The session data object - used for analyzing the current state of the model.</param>
        /// <param name="parallelAngleThreshold">The angle threshold, in Radians, below which two vectors are considered parallel</param>
        /// <param name="centerDistanceThreshold">The distance threshold below which a point will be considered to lie on a plane.</param>
        public CoplanarityInferer(SessionData sessionData,
                                  double parallelAngleThreshold = DEFAULT_PARALLEL_ANGLE_THRESHOLD,
                                  double centerDistanceThreshold = DEFAULT_CENTER_DISTANCE_THRESHOLD,
                                  double centerDistanceRelativeThreshold = DEFAUTL_CENTER_DISTANCE_RELATIVE_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.parallelAngleThreshold = parallelAngleThreshold;
            this.centerDistanceThreshold = centerDistanceThreshold;
            this.centerDistanceRelativeThreshold = centerDistanceRelativeThreshold;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            // we skip coplanarity inference for spheres.
            if (toBeSnapped is NewSphere)
                return Enumerable.Empty<Annotation>();

            var curvesToSkip = toBeAnnotated.FeatureCurves.Concat(GetSphereFeatureCurves()).ToArray();

            var candidates = from firstCurve in toBeAnnotated.FeatureCurves.OfType<CircleFeatureCurve>()
                             from secondCurve in sessionData.FeatureCurves.Except(curvesToSkip).OfType<CircleFeatureCurve>()
                             where AreGoodCandidates(firstCurve, secondCurve)
                             select Tuple.Create(firstCurve, secondCurve);

            if (candidates.Any())
            {
                var annotations =
                    from candidate in candidates
                    let coplanarity = new Coplanarity { Elements = new FeatureCurve[] { candidate.Item1, candidate.Item2 } }
                    select coplanarity as Annotation;

                return annotations;
            }
            else
                return Enumerable.Empty<Annotation>();
        }

        private bool AreGoodCandidates(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            Contract.Requires(firstCurve is CircleFeatureCurve);
            Contract.Requires(secondCurve is CircleFeatureCurve);

            if (firstCurve.IsSameObjectCurve(secondCurve))
                return true; // two curves that are snapped to the same sketch curves are considered coplanar.

            double radiiSum = 0;
            if (firstCurve is CircleFeatureCurve && secondCurve is CircleFeatureCurve)
                radiiSum = (firstCurve as CircleFeatureCurve).RadiusResult + (secondCurve as CircleFeatureCurve).RadiusResult;
            var radiiThreshold = radiiSum * centerDistanceRelativeThreshold;

            bool areAlmostParallel = AreAlmostParallel(firstCurve.NormalResult, secondCurve.NormalResult);

            bool isFirstAlmostOnSecondPlane = IsAlmostOnPlane(point:             firstCurve.CenterResult, 
                                                              planePoint:        secondCurve.CenterResult, 
                                                              planeNormal:       secondCurve.NormalResult,
                                                              relativeThreshold: radiiThreshold);

            bool isSecondAlmostOnFirstPlane = IsAlmostOnPlane(point:             secondCurve.CenterResult, 
                                                              planePoint:        firstCurve.CenterResult, 
                                                              planeNormal:       firstCurve.NormalResult,
                                                              relativeThreshold: radiiThreshold);

            return areAlmostParallel 
                && isFirstAlmostOnSecondPlane 
                && isSecondAlmostOnFirstPlane;
        }

        private bool IsAlmostOnPlane(Point3D point, Point3D planePoint, Vector3D planeNormal, double relativeThreshold)
        {
            var signedDistance = Vector3D.DotProduct(point - planePoint, planeNormal);
            var distance = Math.Abs(signedDistance);
            return distance < centerDistanceThreshold || distance < relativeThreshold;
        }

        private bool AreAlmostParallel(Vector3D u, Vector3D v)
        {
            var uProj = new Vector3D(u.X, -u.Y, 0);
            var vProj = new Vector3D(v.X, -v.Y, 0); 
            var crossLength = Vector3D.CrossProduct(uProj, vProj).Length;
            var angle = Math.Asin(crossLength);
            return angle < DEFAULT_PARALLEL_ANGLE_THRESHOLD;
        }

        private IEnumerable<FeatureCurve> GetSphereFeatureCurves()
        {
            return from primitive in sessionData.SnappedPrimitives
                   where primitive is SnappedSphere
                   from curve in primitive.FeatureCurves
                   select curve;
        }
    }
}
