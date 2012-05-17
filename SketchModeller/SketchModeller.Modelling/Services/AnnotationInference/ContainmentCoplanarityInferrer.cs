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

        private IEnumerable<Annotation> InferCoplanarity(SnappedPrimitive left, SnappedPrimitive right)
        {
            var leftCurves = left.FeatureCurves;
            var rightCurves = right.FeatureCurves;

            var candidates = from leftCurve in left.FeatureCurves
                             from rightCurve in right.FeatureCurves
                             where AreContained(leftCurve, rightCurve)
                             select Tuple.Create(leftCurve, rightCurve);

            if (candidates.Any())
                return SelectBestCandidates(candidates);
            else
                return Enumerable.Empty<Annotation>();
        }

        #region candidate selection

        private IEnumerable<Annotation> SelectBestCandidates(IEnumerable<Tuple<FeatureCurve, FeatureCurve>> candidates)
        {
            // TODO
            return Enumerable.Empty<Annotation>();
        }

        #endregion

        #region containment checking

        private bool AreContained(FeatureCurve leftCurve, FeatureCurve rightCurve)
        {
            return IsContainedIn(leftCurve, rightCurve) ||
                   IsContainedIn(rightCurve, leftCurve);
        }

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
            if (circle.IsFree())
                return Geometry.Empty;

            var circlePoints = ShapeHelper.GenerateCircle(circle.CenterResult, 
                                                          circle.NormalResult, 
                                                          circle.RadiusResult, 
                                                          20);
            return GetProjectedGeometry(circlePoints);
        }

        private Geometry GetGeometry(RectangleFeatureCurve rect)
        {
            if (rect.IsFree())
                return Geometry.Empty;

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
    }
}
