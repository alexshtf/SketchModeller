using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedCylindricalPrimitive : SnappedPrimitive
    {
        public SnappedCylindricalPrimitive()
        {
            FeatureCurves = new FeatureCurve[2];
            
            // top curve
            FeatureCurves[0] = TopFeatureCurve = new CircleFeatureCurve();

            // bottom curve
            FeatureCurves[1] = BottomFeatureCurve = new CircleFeatureCurve();
        }

        #region optimization variables

        public TVec BottomCenter { get; set; }
        public TVec Axis { get; set; }
        public Variable Length { get; set; }

        #endregion

        #region Optimization results

        public Point3D BottomCenterResult { get; set; }
        public Vector3D AxisResult { get; set; }
        public double LengthResult { get; set; }
        
        #endregion

        #region Helper properties

        public Point3D TopCenterResult
        {
            get { return BottomCenterResult + LengthResult * AxisResult; }
        }

        public TVec GetTopCenter()
        {
            return BottomCenter + Length * Axis;
        }

        #endregion

        #region Other properties

        // semantic feature curves
        public CircleFeatureCurve TopFeatureCurve { get; private set; }
        public CircleFeatureCurve BottomFeatureCurve { get; private set; }

        // sketch curves 
        public PointsSequence TopCurve { get; set; }
        public PointsSequence BottomCurve { get; set; }
        public PointsSequence LeftSilhouette { get; set; }
        public PointsSequence RightSilhouette { get; set; }

        #endregion

        public override void UpdateFeatureCurves()
        {
            // update variables
            TopFeatureCurve.Normal = BottomFeatureCurve.Normal = Axis;
            TopFeatureCurve.Center = GetTopCenter();
            BottomFeatureCurve.Center = BottomCenter;

            // update results
            TopFeatureCurve.NormalResult = BottomFeatureCurve.NormalResult = AxisResult;
            TopFeatureCurve.CenterResult = BottomCenterResult + LengthResult * AxisResult;
            BottomFeatureCurve.CenterResult = BottomCenterResult;
        }
    }
}
