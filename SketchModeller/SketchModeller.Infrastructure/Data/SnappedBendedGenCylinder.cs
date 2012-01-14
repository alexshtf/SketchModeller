using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AutoDiff;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedBendedGenCylinder : SnappedPrimitive
    {
        public SnappedBendedGenCylinder()
        {
            FeatureCurves = new FeatureCurve[2];
            // top curve
            FeatureCurves[0] = TopFeatureCurve = new CircleFeatureCurve();

            // bottom curve
            FeatureCurves[1] = BottomFeatureCurve = new CircleFeatureCurve();
        }
        public SnappedBendedCylinderComponent[] Components { get; set; }
        public BendedCylinderComponent[] ComponentResults { get; set; }

        #region optimization variables

        public TVec BottomCenter { get; set; }   
        public TVec TopCenter { get; set; }
        public TVec NPtop { get; set; }
        public TVec NPbot { get; set; }
        public TVec U { get; set; }
        public TVec V { get; set; }
        
        #endregion
        
        #region Optimization results

        public Point3D BottomCenterResult { get; set; }
        public Point3D TopCenterResult { get; set; }
        public Vector NPtopResult { get; set; }
        public Vector NPbotResult { get; set; }
        public Vector3D Uresult { get; set; }
        public Vector3D Vresult { get; set; }
        
        
        #endregion

        #region Other properties

        // semantic feature curves
        public CircleFeatureCurve TopFeatureCurve { get; private set; }
        public CircleFeatureCurve BottomFeatureCurve { get; private set; }

        // sketch curves 
        public PointsSequence LeftSilhouette { get; set; }
        public PointsSequence RightSilhouette { get; set; }

        #endregion

        public override void UpdateFeatureCurves()
        {
            // update variables
            BottomFeatureCurve.Center = BottomCenter + Components.First().vS * U + Components.First().vT * V;
            TopFeatureCurve.Center = TopCenter = BottomCenter + Components.Last().vS * U + Components.Last().vT * V;
            BottomFeatureCurve.Normal = NPbot.X * U + NPbot.Y * V;
            TopFeatureCurve.Normal = NPtop.X * U + NPtop.Y * V;
            TopFeatureCurve.Radius = Components.Last().Radius;
            BottomFeatureCurve.Radius = Components.First().Radius;

            // update results
            BottomFeatureCurve.CenterResult = BottomCenterResult = BottomCenterResult + ComponentResults.First().S * Uresult + ComponentResults.First().T * Vresult;
            TopFeatureCurve.CenterResult = TopCenterResult = TopCenterResult + ComponentResults.Last().S * Uresult + ComponentResults.Last().T * Vresult;
            BottomFeatureCurve.NormalResult = NPtopResult.X * Uresult + NPbotResult.Y * Vresult;
            TopFeatureCurve.NormalResult = NPtopResult.X * Uresult + NPtopResult.Y * Vresult;
            TopFeatureCurve.RadiusResult = ComponentResults.Last().Radius;
            BottomFeatureCurve.RadiusResult = ComponentResults.First().Radius;
        }

    }

}