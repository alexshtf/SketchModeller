using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using System.Windows;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class NewCylindricalPrimitive : NewPrimitive
    {
        public NewCylindricalPrimitive()
        {
            FeatureCurves = ArrayUtils.Generate<PrimitiveCurve>(2);
            SilhouetteCurves = ArrayUtils.Generate<PrimitiveCurve>(2);

            Center = new PointParameter();
            Axis = new VectorParameter();
            Length = new ValueParameter();

            RegisterParameter(() => Center);
            RegisterParameter(() => Axis);
            RegisterParameter(() => Length);
        }

        public PointParameter Center { get; private set; }
        public VectorParameter Axis { get; private set; }
        public ValueParameter Length { get; private set; }

        public Point3D Top
        {
            get { return Center.Value + 0.5 * Length.Value * Axis.Value; }
        }

        public Point3D Bottom
        {
            get { return Center.Value - 0.5 * Length.Value * Axis.Value; }
        }

        #region Primitive curves

        public PrimitiveCurve TopCircle
        {
            get { return FeatureCurves[0]; }
        }

        public PrimitiveCurve BottomCircle
        {
            get { return FeatureCurves[1]; }
        }

        public PrimitiveCurve LeftSilhouette
        {
            get { return SilhouetteCurves[0]; }
        }

        public PrimitiveCurve RightSilhouette
        {
            get { return SilhouetteCurves[1]; }
        }

        #endregion

        protected abstract double TopRadiusInternal { get; }
        protected abstract double BottomRadiusInternal { get; }

        public override void UpdateCurvesGeometry()
        {
            // get projected versions of top/bottom circles
            var top = Center.Value + 0.5 * Length.Value * Axis.Value;
            var topCircle3d = ShapeHelper.GenerateCircle(top, Axis, TopRadiusInternal, 20);
            var topCircle = ShapeHelper.ProjectCurve(topCircle3d);

            var bottom = Center.Value - 0.5 * Length.Value * Axis.Value;
            var bottomCircle3d = ShapeHelper.GenerateCircle(bottom, Axis, BottomRadiusInternal, 20);
            var bottomCircle = ShapeHelper.ProjectCurve(bottomCircle3d);

            // find the axis in projected coordinates
            var tb = ShapeHelper.ProjectCurve(top, bottom);
            var axis2d = tb[0] - tb[1];

            // find the 2 silhouette lines
            var perp = new Vector(axis2d.Y, -axis2d.X);
            perp.Normalize();
            var lt = tb[0] + TopRadiusInternal * perp;
            var lb = tb[1] + BottomRadiusInternal * perp;
            var rt = tb[0] - TopRadiusInternal * perp;
            var rb = tb[1] - BottomRadiusInternal * perp;

            var leftLine = new Point[] { lt, lb };
            var rightLine = new Point[] { rt, rb };

            TopCircle.Points = topCircle;
            BottomCircle.Points = bottomCircle;
            LeftSilhouette.Points = leftLine;
            RightSilhouette.Points = rightLine;
        }
    }
}
