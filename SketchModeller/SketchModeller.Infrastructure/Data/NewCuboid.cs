using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data.EditConstraints;
using System.Windows.Media.Media3D;
using Utils;

namespace SketchModeller.Infrastructure.Data
{
    public class NewCuboid : NewPrimitive
    {
        public NewCuboid()
        {
            FeatureCurves = ArrayUtils.Generate<EnhancedPrimitiveCurve>(12);
            SilhouetteCurves = ArrayUtils.Generate<PrimitiveCurve>(0);
            //The Center of the Cube
            Center = new PointParameter();
            //The <H, W, D> is the orthonomal system alligned to the Height Width and Depth of the Cuboid
            //
            H = new VectorParameter();
            W = new VectorParameter();
            D = new VectorParameter();
            Height = new ValueParameter();
            Width = new ValueParameter();
            Depth = new ValueParameter();

            RegisterParameter(() => Center);
            RegisterParameter(() => H);
            RegisterParameter(() => W);
            RegisterParameter(() => D);
            RegisterParameter(() => Height);
            RegisterParameter(() => Width);
            RegisterParameter(() => Depth);

            ActiveCubicCorner = -1;
        }
        public int ActiveCubicCorner { get; set; }

        public PrimitiveCurve[][] ArrayOfCorners
        {
            get
            {
                return new PrimitiveCurve[][] { LUFcubicCorner, RUFcubicCorner, LUBcubicCorner, RUBcubicCorner, 
                                                  LDFcubicCorner, RDFcubicCorner, LDBcubicCorner, RDBcubicCorner};
            }
        }
        public PointParameter Center { get; set; }
        public VectorParameter H { get; set; }
        public VectorParameter W { get; set; }
        public VectorParameter D { get; set; }
        public ValueParameter Height { get; set; }
        public ValueParameter Width { get; set; }
        public ValueParameter Depth { get; set; }

        public override void UpdateCurvesGeometry()
        {
            Func<double, double, double, Point3D> createPointInBasis = (w, h, d) =>
                Center.Value + w * W.Value + h * H.Value + d * D.Value;

            //Now comes the tedious job to draw the feature curves of the cuboid
            //Define the vertices of the cube centered at the center of the coordinate system 
            Point3D vA = createPointInBasis(-0.5 * Width, 0.5 * Height, 0.5 * Depth);
            Point3D vB = createPointInBasis(0.5 * Width, 0.5 * Height, 0.5 * Depth);
            Point3D vC = createPointInBasis(0.5 * Width, -0.5 * Height, 0.5 * Depth);
            Point3D vD = createPointInBasis(-0.5 * Width, -0.5 * Height, 0.5 * Depth);
            Point3D vE = createPointInBasis(-0.5 * Width, 0.5 * Height, -0.5 * Depth);
            Point3D vF = createPointInBasis(0.5 * Width, 0.5 * Height, -0.5 * Depth);
            Point3D vG = createPointInBasis(0.5 * Width, -0.5 * Height, -0.5 * Depth);
            Point3D vH = createPointInBasis(-0.5 * Width, -0.5 * Height, -0.5 * Depth);

            FeatureCurves[0].Points = ShapeHelper.ProjectCurve(vA, vE);
            FeatureCurves[1].Points = ShapeHelper.ProjectCurve(vA, vB);
            FeatureCurves[2].Points = ShapeHelper.ProjectCurve(vB, vF);
            FeatureCurves[3].Points = ShapeHelper.ProjectCurve(vE, vF);
            FeatureCurves[4].Points = ShapeHelper.ProjectCurve(vD, vH);
            FeatureCurves[5].Points = ShapeHelper.ProjectCurve(vC, vD);
            FeatureCurves[6].Points = ShapeHelper.ProjectCurve(vC, vG);
            FeatureCurves[7].Points = ShapeHelper.ProjectCurve(vG, vH);
            FeatureCurves[8].Points = ShapeHelper.ProjectCurve(vA, vD);
            FeatureCurves[9].Points = ShapeHelper.ProjectCurve(vB, vC);
            FeatureCurves[10].Points = ShapeHelper.ProjectCurve(vF, vG);
            FeatureCurves[11].Points = ShapeHelper.ProjectCurve(vE, vH);
            EnhancedPrimitiveCurve ec = (EnhancedPrimitiveCurve)FeatureCurves[0];
            ec.Points3D = new Point3D[] { vA, vE };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[1];
            ec.Points3D = new Point3D[] { vA, vB };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[2];
            ec.Points3D = new Point3D[] { vB, vF };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[3];
            ec.Points3D = new Point3D[] { vE, vF };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[4];
            ec.Points3D = new Point3D[] { vD, vH };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[5];
            ec.Points3D = new Point3D[] { vC, vD };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[6];
            ec.Points3D = new Point3D[] { vC, vG };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[7];
            ec.Points3D = new Point3D[] { vG, vH };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[8];
            ec.Points3D = new Point3D[] { vA, vD };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[9];
            ec.Points3D = new Point3D[] { vB, vC };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[10];
            ec.Points3D = new Point3D[] { vF, vG };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[11];
            ec.Points3D = new Point3D[] { vE, vH };
        }

        #region corner properties - each corner is the set of feature curves that intersect there

        private PrimitiveCurve[] LUFcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[1], FeatureCurves[8], FeatureCurves[0] }; }
        }

        private PrimitiveCurve[] RUFcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[1], FeatureCurves[9], FeatureCurves[2] }; }
        }

        private PrimitiveCurve[] LUBcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[3], FeatureCurves[11], FeatureCurves[0] }; }
        }

        private PrimitiveCurve[] RUBcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[3], FeatureCurves[10], FeatureCurves[2] }; }
        }

        private PrimitiveCurve[] LDFcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[5], FeatureCurves[8], FeatureCurves[4] }; }
        }

        private PrimitiveCurve[] RDFcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[5], FeatureCurves[9], FeatureCurves[6] }; }
        }

        private PrimitiveCurve[] LDBcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[7], FeatureCurves[11], FeatureCurves[4] }; }
        }

        private PrimitiveCurve[] RDBcubicCorner
        {
            get { return new PrimitiveCurve[] { FeatureCurves[7], FeatureCurves[10], FeatureCurves[6] }; }
        }

        #endregion
    }
}
