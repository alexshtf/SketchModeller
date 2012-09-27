using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedCuboid : SnappedPrimitive
    {
        public SnappedCuboid()
        {
            FeatureCurves = new FeatureCurve[6];
            FeatureCurves[0] = TopPlane = new RectangleFeatureCurve();
            FeatureCurves[1] = BotPlane = new RectangleFeatureCurve();
            FeatureCurves[2] = LftPlane = new RectangleFeatureCurve();
            FeatureCurves[3] = RgtPlane = new RectangleFeatureCurve();
            FeatureCurves[4] = FrnPlane = new RectangleFeatureCurve();
            FeatureCurves[5] = BakPlane = new RectangleFeatureCurve();
        }

        #region set Semantinc Feature Curves

        // semantic feature curves
        public RectangleFeatureCurve TopPlane { get; private set; }
        public RectangleFeatureCurve BotPlane { get; private set; }
        public RectangleFeatureCurve LftPlane { get; private set; }
        public RectangleFeatureCurve RgtPlane { get; private set; }
        public RectangleFeatureCurve FrnPlane { get; private set; }
        public RectangleFeatureCurve BakPlane { get; private set; }

        #endregion

        #region optimization variables

        public TVec Center { get; set; }
        public Variable Width { get; set; }
        public Variable Height { get; set; }
        public Variable Depth { get; set; }
        public TVec Wv { get; set; }
        public TVec Hv { get; set; }
        public TVec Dv { get; set; }

        #endregion

        #region optimization results

        public Point3D CenterResult { get; set; }
        public double WidthResult { get; set; }
        public double HeightResult { get; set; }
        public double DepthResult { get; set; }
        public Vector3D Wresult { get; set; }
        public Vector3D Hresult { get; set; }
        public Vector3D Dresult { get; set; }

        #endregion

        #region Cubic Corner

        public PrimitiveCurve[] CubicCorner { get; set; }
        public int CubicCornerIdx { get; set; }

        #endregion

        #region Debugging

        public Vector3D W { get; set; }
        public Vector3D H { get; set; }
        public Vector3D D { get; set; }
        public Point3D Origin { get; set; }

        #endregion

        public override void UpdateFeatureCurves()
        {
            // update variables
            TopPlane.Center = Center + 0.5 * Hv * Height;
            TopPlane.Normal = Hv;
            TopPlane.WidthVector = Dv;
            TopPlane.Widgth = Depth;
            TopPlane.Height = Width;

            BotPlane.Center = Center - 0.5 * Hv * Height;
            BotPlane.Normal = -Hv;
            BotPlane.WidthVector = Dv;
            BotPlane.Widgth = Depth;
            BotPlane.Height = Width;

            LftPlane.Center = Center - 0.5 * Wv * Width;
            LftPlane.Normal = -Wv;
            LftPlane.WidthVector = Hv;
            LftPlane.Widgth = Height;
            LftPlane.Height = Depth;

            RgtPlane.Center = Center + 0.5 * Wv * Width;
            RgtPlane.Normal = Wv;
            RgtPlane.WidthVector = Hv;
            RgtPlane.Widgth = Height;
            RgtPlane.Height = Depth;

            FrnPlane.Center = Center + 0.5 * Dv * Depth;
            FrnPlane.Normal = Dv;
            FrnPlane.WidthVector = Wv;
            FrnPlane.Widgth = Width;
            FrnPlane.Height = Height;

            BakPlane.Center = Center - 0.5 * Dv * Depth;
            BakPlane.Normal = -Dv;
            BakPlane.WidthVector = Wv;
            BakPlane.Widgth = Width;
            BakPlane.Height = Height;

            // update results
            TopPlane.CenterResult = CenterResult + 0.5 * Hresult * HeightResult;
            TopPlane.NormalResult = Hresult;
            TopPlane.WidthVectorResult = Dresult;
            TopPlane.WidthResult = DepthResult;
            TopPlane.HeightResult = WidthResult;

            BotPlane.CenterResult = CenterResult - 0.5 * Hresult * HeightResult;
            BotPlane.NormalResult = -Hresult;
            BotPlane.WidthVectorResult = Dresult;
            BotPlane.WidthResult = DepthResult;
            BotPlane.HeightResult = WidthResult;

            LftPlane.CenterResult = CenterResult - 0.5 * Wresult * WidthResult;
            LftPlane.NormalResult = -Wresult;
            LftPlane.WidthVectorResult = Hresult;
            LftPlane.WidthResult = HeightResult;
            LftPlane.HeightResult = DepthResult;

            RgtPlane.CenterResult = CenterResult + 0.5 * Wresult * WidthResult;
            RgtPlane.NormalResult = Wresult;
            RgtPlane.WidthVectorResult = Hresult;
            RgtPlane.WidthResult = HeightResult;
            RgtPlane.HeightResult = DepthResult;

            FrnPlane.CenterResult = CenterResult + 0.5 * Dresult * DepthResult;
            FrnPlane.NormalResult = Dresult;
            FrnPlane.WidthVectorResult = Wresult;
            FrnPlane.WidthResult = WidthResult;
            FrnPlane.HeightResult = HeightResult;

            BakPlane.CenterResult = CenterResult - 0.5 * Dresult * DepthResult;
            BakPlane.NormalResult = -Dresult;
            BakPlane.WidthVectorResult = Wresult;
            BakPlane.WidthResult = WidthResult;
            BakPlane.HeightResult = HeightResult;
        }
    }
}
