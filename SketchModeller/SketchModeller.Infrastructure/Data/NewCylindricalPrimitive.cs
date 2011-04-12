using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;

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
    }
}
