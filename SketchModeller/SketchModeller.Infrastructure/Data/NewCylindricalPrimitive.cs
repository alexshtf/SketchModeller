using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class NewCylindricalPrimitive : NewPrimitive
    {
        public NewCylindricalPrimitive()
        {
            FeatureCurves = ArrayUtils.Generate<PrimitiveCurve>(2);
            SilhouetteCurves = ArrayUtils.Generate<PrimitiveCurve>(2);
        }

        #region Center property

        private Point3D center;

        public Point3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
            }
        }

        #endregion

        #region Axis property

        private Vector3D axis;

        public Vector3D Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                RaisePropertyChanged(() => Axis);
            }
        }

        #endregion

        #region Length property

        private double length;

        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
            }
        }

        #endregion

        public Point3D Top
        {
            get { return Center + 0.5 * Length * Axis; }
        }

        public Point3D Bottom
        {
            get { return Center - 0.5 * Length * Axis; }
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
            var top = Center + 0.5 * Length * Axis;
            var topCircle3d = ShapeHelper.GenerateCircle(top, Axis, TopRadiusInternal, 10);
            var topCircle = ShapeHelper.ProjectCurve(topCircle3d);

            var bottom = Center - 0.5 * Length * Axis;
            var bottomCircle3d = ShapeHelper.GenerateCircle(bottom, Axis, BottomRadiusInternal, 10);
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
