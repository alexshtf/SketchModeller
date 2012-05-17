using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using SketchModeller.Infrastructure;
using System.Windows.Media.Media3D;
using System.Collections;
using Utils;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    /// <summary>
    /// Infers coplanarity constraints based on 2D feature curve containment.
    /// </summary>
    class ContainmentCoplanarityInferrer : IInferrer
    {
        private readonly SessionData sessionData;

        public ContainmentCoplanarityInferrer(SessionData sessionData)
        {
            this.sessionData = sessionData;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            var otherPrimitives = from primitive in sessionData.SnappedPrimitives
                                  where primitive != toBeAnnotated
                                  select primitive;

            var result =
                from primitive in otherPrimitives
                from annotation in InferCoplanarity(primitive, toBeAnnotated)
                select annotation;

            return result;
        }

        private IEnumerable<Annotation> InferCoplanarity(SnappedPrimitive existing, SnappedPrimitive fresh)
        {
            // spheres do not particiate here
            if (existing is SnappedSphere || fresh is SnappedSphere)
                return Enumerable.Empty<Annotation>();

            var candidates = GetCandidates(existing.FeatureCurves, fresh.FeatureCurves);
            return SelectBestCandidates(candidates);
        }

        #region candidate selection

        private IEnumerable<Annotation> SelectBestCandidates(IEnumerable<CandidatePair> candidates)
        {
            // eliminate containers that are not visible because their normal faces away from the viewer
            // (positive Z coordinate).
            var withVisibleContainers = from pair in candidates
                                        where pair.Container.NormalResult.Z < 0
                                        select pair;

            if (withVisibleContainers.Any())
            {
                // choose the pair such that the projection of the containee's center on the container's normal 
                // has the lowest value
                var bestCandidate = candidates.Minimizer(ContainedCenterOnContainerAxisProjection);

                // construct the coplanarity annotation and return a singleton enumerable containing it.
                var annotation = new Coplanarity { Elements = new FeatureCurve[] { bestCandidate.Container, bestCandidate.Contained } };
                return Utils.Enumerable.Singleton(annotation);
            }
            else
                return Enumerable.Empty<Annotation>();
        }

        private double ContainedCenterOnContainerAxisProjection(CandidatePair pair)
        {
            var containedCenter = pair.Contained.CenterResult;
            var containerAxis = pair.Container.NormalResult;
            var containerCenter = pair.Container.CenterResult;

            // this is t such that the projected point is (containerCenter + t * containerAxis);
            var projectionParameter = MathUtils3D.ProjectOnLine(containedCenter, containerCenter, containerAxis).Item1;

            return projectionParameter;
        }

        #endregion

        #region Candidates list construction

        private IEnumerable<CandidatePair> GetCandidates(FeatureCurve[] existingCurves, FeatureCurve[] freshCurves)
        {
            var existingGeometries = existingCurves.Select(curve => GetGeometry(curve)).ToArray();
            var freshGeometries = freshCurves.Select(curve => GetGeometry(curve)).ToArray();
            var allGeometries = existingGeometries.Concat(freshGeometries).ToArray();

            var candidates1 = from existingCurve in existingCurves
                              from freshCurve in freshCurves
                              where IsContainedIn(existingCurve, freshCurve)
                              select new CandidatePair { Contained = existingCurve, Container = freshCurve };

            var candidates2 = from existingCurve in existingCurves
                              from freshCurve in freshCurves
                              where IsContainedIn(freshCurve, existingCurve)
                              select new CandidatePair { Contained = freshCurve, Container = existingCurve };
            var candidates = candidates1.Union(candidates2);

            candidates = candidates.ToArray();
            return candidates;
        }


        #endregion

        #region containment checking

        private bool IsContainedIn(FeatureCurve containee, FeatureCurve container)
        {
            Contract.Requires(containee is RectangleFeatureCurve || containee is CircleFeatureCurve);
            Contract.Requires(container is RectangleFeatureCurve || container is CircleFeatureCurve);

            var containeeGeometry = GetGeometry(containee);
            var containerGeometry = GetGeometry(container);

            return containerGeometry.FillContains(containeeGeometry);
        }

        private Geometry GetGeometry(FeatureCurve curve)
        {
            Contract.Requires(curve is CircleFeatureCurve || curve is RectangleFeatureCurve);

            var circle = curve as CircleFeatureCurve;
            var rect = curve as RectangleFeatureCurve;

            if (circle != null)
                return GetGeometry(circle);
            else
                return GetGeometry(rect);
        }

        private Geometry GetGeometry(CircleFeatureCurve circle)
        {
            var circlePoints = ShapeHelper.GenerateCircle(circle.CenterResult, 
                                                          circle.NormalResult, 
                                                          circle.RadiusResult, 
                                                          20);
            return GetProjectedGeometry(circlePoints);
        }

        private Geometry GetGeometry(RectangleFeatureCurve rect)
        {
            var rectPoints = ShapeHelper.GenerateRectangle(rect.CenterResult,
                                                           rect.NormalResult,
                                                           rect.WidthVectorResult,
                                                           rect.WidthResult,
                                                           rect.HeightResult);
            return GetProjectedGeometry(rectPoints);
        }

        private static Geometry GetProjectedGeometry(Point3D[] points3d)
        {
            var projectedPoints = ShapeHelper.ProjectCurve(points3d);

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(projectedPoints[0], true, true);
                ctx.PolyLineTo(projectedPoints.Skip(1).ToList(), true, true);
                ctx.Close();
            }

            return geometry;
        }

        #endregion

        #region CandidatePair class

        private struct CandidatePair
        {
            public FeatureCurve Container { get; set; }
            public FeatureCurve Contained { get; set; }
        }

        #endregion
    }
}
