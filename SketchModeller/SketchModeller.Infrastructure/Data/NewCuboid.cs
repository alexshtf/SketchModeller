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
            //Now comes the tedious job to draw the feature curves of the cuboid
            //Define the vertices of the cube centered at the center of the coordinate system 
            Point3D A = new Point3D(-0.5 * Width, 0.5 * Height, 0.5 * Depth);
            Point3D B = new Point3D(0.5 * Width, 0.5 * Height, 0.5 * Depth);
            Point3D C = new Point3D(0.5 * Width, -0.5 * Height, 0.5 * Depth);
            Point3D Dc = new Point3D(-0.5 * Width, -0.5 * Height, 0.5 * Depth);
            Point3D E = new Point3D(-0.5 * Width, 0.5 * Height, -0.5 * Depth);
            Point3D F = new Point3D(0.5 * Width, 0.5 * Height, -0.5 * Depth);
            Point3D G = new Point3D(0.5 * Width, -0.5 * Height, -0.5 * Depth);
            Point3D Hc = new Point3D(-0.5 * Width, -0.5 * Height, -0.5 * Depth);
            Point3D[] Pnts = new Point3D[] { A, B, C, Dc, E, F, G, Hc };
            double[][] P = FindTransformationMatrix(W, H, D);
            TransformPoints(Pnts, Center, P);

            A = Pnts[0];
            B = Pnts[1];
            C = Pnts[2];
            Dc = Pnts[3];
            E = Pnts[4];
            F = Pnts[5];
            G = Pnts[6];
            Hc = Pnts[7];

            FeatureCurves[0].Points = ShapeHelper.ProjectCurve(A, E);
            FeatureCurves[1].Points = ShapeHelper.ProjectCurve(A, B);
            FeatureCurves[2].Points = ShapeHelper.ProjectCurve(B, F);
            FeatureCurves[3].Points = ShapeHelper.ProjectCurve(E, F);
            FeatureCurves[4].Points = ShapeHelper.ProjectCurve(Dc, Hc);
            FeatureCurves[5].Points = ShapeHelper.ProjectCurve(C, Dc);
            FeatureCurves[6].Points = ShapeHelper.ProjectCurve(C, G);
            FeatureCurves[7].Points = ShapeHelper.ProjectCurve(G, Hc);
            FeatureCurves[8].Points = ShapeHelper.ProjectCurve(A, Dc);
            FeatureCurves[9].Points = ShapeHelper.ProjectCurve(B, C);
            FeatureCurves[10].Points = ShapeHelper.ProjectCurve(F, G);
            FeatureCurves[11].Points = ShapeHelper.ProjectCurve(E, Hc);
            EnhancedPrimitiveCurve ec = (EnhancedPrimitiveCurve)FeatureCurves[0];
            ec.Points3D = new Point3D[] { A, E };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[1];
            ec.Points3D = new Point3D[] { A, B };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[2];
            ec.Points3D = new Point3D[] { B, F };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[3];
            ec.Points3D = new Point3D[] { E, F };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[4];
            ec.Points3D = new Point3D[] { Dc, Hc };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[5];
            ec.Points3D = new Point3D[] { C, Dc };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[6];
            ec.Points3D = new Point3D[] { C, G };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[7];
            ec.Points3D = new Point3D[] { G, Hc };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[8];
            ec.Points3D = new Point3D[] { A, Dc };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[9];
            ec.Points3D = new Point3D[] { B, C };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[10];
            ec.Points3D = new Point3D[] { F, G };
            ec = (EnhancedPrimitiveCurve)FeatureCurves[11];
            ec.Points3D = new Point3D[] { E, Hc };
        }
        
        public static void TransformPoints(Point3D[] points, Point3D Center, double[][] P)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Point3D Pt = new Point3D();
                Pt.X = points[i].X * P[0][0] + points[i].Y * P[1][0] + points[i].Z * P[2][0] + Center.X;
                Pt.Y = points[i].X * P[0][1] + points[i].Y * P[1][1] + points[i].Z * P[2][1] + Center.Y;
                Pt.Z = points[i].X * P[0][2] + points[i].Y * P[1][2] + points[i].Z * P[2][2] + Center.Z;
                points[i] = new Point3D(Pt.X, Pt.Y, Pt.Z);
            }
        }
        public static double[][] FindTransformationMatrix(Vector3D W, Vector3D H, Vector3D D)
        {
            double[][] G = new double[3][];
            for (int i = 0; i < 3; i++) G[i] = new double[6];
            G[0][0] = W.X; G[0][1] = H.X; G[0][2] = D.X; G[0][3] = 1; G[0][4] = 0; G[0][5] = 0;
            G[1][0] = W.Y; G[1][1] = H.Y; G[1][2] = D.Y; G[1][3] = 0; G[1][4] = 1; G[1][5] = 0;
            G[2][0] = W.Z; G[2][1] = H.Z; G[2][2] = D.Z; G[2][3] = 0; G[2][4] = 0; G[2][5] = 1;
            for (int j = 0; j < 3; j++)
            {
                int temp = j;

                /* finding maximum coefficient of Xj in last (noofequations-j) equations */

                for (int i = j + 1; i < 3; i++)
                    if (G[i][j] > G[temp][j])
                        temp = i;


                /* swapping row which has maximum coefficient of Xj */

                if (temp != j)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        var temporary = G[j][k];
                        G[j][k] = G[temp][k];
                        G[temp][k] = temporary;
                    }
                }

                /* performing row operations to form required diagonal matrix */

                for (int i = 0; i < 3; i++)
                    if (i != j)
                    {
                        var r = G[i][j];
                        for (int k = 0; k < 6; k++)
                            G[i][k] -= (G[j][k] / G[j][j]) * r;
                    }
            }
            for (int j = 0; j < 3; j++)
            {
                var pivot = G[j][j];
                for (int i = 0; i < 6; i++)
                    G[j][i] /= pivot;
            }
            double[][] P = new double[3][];
            for (int i = 0; i < 3; i++) P[i] = new double[3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    P[i][j] = G[i][j + 3];
            return P;
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
