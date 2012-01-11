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

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    /// <summary>
    /// Infers coplanarity constraints based on a simple heuristic.
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
        public const double DEFAULT_PARALLEL_ANGLE_THRESHOLD = 15 * Math.PI / 180; // 20 degrees
        public const double DEFAULT_CENTER_DISTANCE_THRESHOLD = 0.05;

        private readonly SessionData sessionData;
        private readonly double parallelAngleThreshold; 
        private readonly double centerDistanceThreshold;

        /// <summary>
        /// Constructs a new instance of <see cref="CoplanarityInferer"/> class.
        /// </summary>
        /// <param name="sessionData">The session data object - used for analyzing the current state of the model.</param>
        /// <param name="parallelAngleThreshold">The angle threshold, in Radians, below which two vectors are considered parallel</param>
        /// <param name="centerDistanceThreshold">The distance threshold below which a point will be considered to lie on a plane.</param>
        public CoplanarityInferer(SessionData sessionData,
                                  double parallelAngleThreshold = DEFAULT_PARALLEL_ANGLE_THRESHOLD,
                                  double centerDistanceThreshold = DEFAULT_CENTER_DISTANCE_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.parallelAngleThreshold = parallelAngleThreshold;
            this.centerDistanceThreshold = centerDistanceThreshold;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            // we skip coplanarity inference for spheres.
            if (toBeSnapped is NewSphere)
                return Enumerable.Empty<Annotation>();

            var curvesToSkip = toBeAnnotated.FeatureCurves.Concat(GetSphereFeatureCurves()).ToArray();

            var candidates = from firstCurve in toBeAnnotated.FeatureCurves
                             from secondCurve in sessionData.FeatureCurves.Except(curvesToSkip)
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
            bool areAlmostParallel = AreAlmostParallel(firstCurve.NormalResult, secondCurve.NormalResult);

            bool isFirstAlmostOnSecondPlane = IsAlmostOnPlane(point:       firstCurve.CenterResult, 
                                                              planePoint:  secondCurve.CenterResult, 
                                                              planeNormal: secondCurve.NormalResult);

            bool isSecondAlmostOnFirstPlane = IsAlmostOnPlane(point:       secondCurve.CenterResult, 
                                                              planePoint:  firstCurve.CenterResult, 
                                                              planeNormal: firstCurve.NormalResult);

            return areAlmostParallel 
                && isFirstAlmostOnSecondPlane 
                && isSecondAlmostOnFirstPlane;
        }

        private bool IsAlmostOnPlane(Point3D point, Point3D planePoint, Vector3D planeNormal)
        {
            var signedDistance = Vector3D.DotProduct(point - planePoint, planeNormal);
            var distance = Math.Abs(signedDistance);
            return distance < centerDistanceThreshold;
        }

        private bool AreAlmostParallel(Vector3D u, Vector3D v)
        {
            var crossLength = Vector3D.CrossProduct(u, v).Length;
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
