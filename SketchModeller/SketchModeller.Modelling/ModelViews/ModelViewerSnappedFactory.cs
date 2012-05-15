using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory : IVisual3DFactory
    {
        public static readonly ModelViewerSnappedFactory Instance = new ModelViewerSnappedFactory();
        public static readonly Brush FRONT_BRUSH = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA6C1D0"));
        public static readonly Brush FRONT_BRUSH_MARKED = Brushes.LightSkyBlue;
        public static readonly Brush BACK_BRUSH = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB5AFD8"));
        public static readonly Brush BACK_SELECTED_BRUSH = Brushes.Orange;

        public Visual3D Create(object item)
        {
            Visual3D result = null;
            item.MatchClass<SnappedCylinder>(cylinderData => result = CreateCylinderView(cylinderData));
            item.MatchClass<SnappedCone>(coneData => result = CreateConeView(coneData));
            item.MatchClass<SnappedSphere>(sphereData => result = CreateSphereView(sphereData));
            item.MatchClass<SnappedStraightGenCylinder>(sgc => result = CreateSgcView(sgc));
            item.MatchClass<SnappedBendedGenCylinder>(bgc => result = CreateBgcView(bgc));
            item.MatchClass<SnappedCuboid>(cuboidData => result = CreateCuboidView(cuboidData));

            Contract.Assume(result != null);
            PrimitivesPickService.SetPrimitiveData(result, item as SnappedPrimitive);
            return result;
        }

        private static ModelVisual3D CreateVisual(SnappedPrimitive snappedPrimitive, Action<GeometryModel3D> geometryModelAction)
        {
            var frontEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var frontDiffusePart = new DiffuseMaterial { Brush = FRONT_BRUSH };
            var frontMaterial = new MaterialGroup { Children = { frontEmissivePart, frontDiffusePart } };

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D();
            model3d.Material = frontMaterial;

            var backEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var backDiffusePart = new DiffuseMaterial { Brush = BACK_BRUSH };
            var backMaterial = new MaterialGroup { Children = { backEmissivePart, backDiffusePart } };
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;

            geometryModelAction(model3d);

            CreateFeatureCurves(visual, snappedPrimitive.FeatureCurves);
            return visual;
        }

        private static ModelVisual3D CreateVisual(MeshGeometry3D geometry, SnappedPrimitive snappedPrimitive, bool createFeatureCurves = true)
        {
            var frontEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var frontDiffusePart = new DiffuseMaterial { Brush = FRONT_BRUSH };
            var frontMaterial = new MaterialGroup { Children = { frontEmissivePart, frontDiffusePart } };

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D(
                geometry,
                frontMaterial);

            var backEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var backDiffusePart = new DiffuseMaterial { Brush = BACK_BRUSH };
            var backMaterial = new MaterialGroup { Children = { backEmissivePart, backDiffusePart } };
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;
            if (createFeatureCurves) 
                CreateFeatureCurves(visual, snappedPrimitive.FeatureCurves);
            
            return visual;
        }

        private static ModelVisual3D CreateVisual(MeshGeometry3D geometry, Brush brush)
        {
            var frontEmissivePart = new EmissiveMaterial { Brush = brush };//CreateEmissiveMaterial(snappedPrimitive);
            var frontDiffusePart = new DiffuseMaterial { Brush = brush };
            var frontMaterial = new MaterialGroup { Children = { frontEmissivePart, frontDiffusePart } };

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D(
                geometry,
                frontMaterial);
            //model3d.
            var backEmissivePart = new EmissiveMaterial { Brush = brush }; //CreateEmissiveMaterial(snappedPrimitive);
            var backDiffusePart = new DiffuseMaterial { Brush = brush };
            var backMaterial = new MaterialGroup { Children = { backEmissivePart, backDiffusePart } };
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;
            return visual;
        }

        private static EmissiveMaterial CreateEmissiveMaterial(SnappedPrimitive snappedPrimitive)
        {
            var frontEmissivePart = new EmissiveMaterial();
            frontEmissivePart.Bind(
                EmissiveMaterial.BrushProperty,
                () => snappedPrimitive.IsSelected,
                flag => flag ? Brushes.LightGray : Brushes.Gray);
            return frontEmissivePart;
        }

        private static void CreateFeatureCurves(ModelVisual3D root, FeatureCurve[] featureCurves)
        {
            foreach (var item in featureCurves)
            {
                ModelVisual3D featureCurveVisual = null;
                item.MatchClass<CircleFeatureCurve>(cfc => featureCurveVisual = new CircleFeatureVisual(cfc));
                item.MatchClass<RectangleFeatureCurve>(rfc => featureCurveVisual = new RectangleFeatureVisual(rfc));
                Debug.Assert(featureCurveVisual != null, "Unknown feature curve type");

                root.Children.Add(featureCurveVisual);
            }
        }

        private static Visual3D CreateCylinderView(Point3D[] topPoints, Point3D[] botPoints, SnappedPrimitive snappedPrimitive)
        {
            Contract.Requires(topPoints.Length == botPoints.Length);
            Contract.Requires(snappedPrimitive != null);

            var m = topPoints.Length;

            // top points indices [0 .. m-1]
            var topIdx = System.Linq.Enumerable.Range(0, m).ToArray();

            // bottom points indices [m .. 2*m - 1]
            var bottomIdx = System.Linq.Enumerable.Range(m, m).ToArray();
            Contract.Assume(topIdx.Length == bottomIdx.Length);

            // create cylinder geometry
            var geometry = new MeshGeometry3D();
            geometry.Positions = new Point3DCollection(topPoints.Concat(botPoints));
            geometry.TriangleIndices = new Int32Collection();
            for (int i = 0; i < m; ++i)
            {
                var j = (i + 1) % m;
                var pc = topIdx[i];
                var pn = topIdx[j];
                var qc = bottomIdx[i];
                var qn = bottomIdx[j];

                geometry.TriangleIndices.AddMany(pc, qc, pn);
                geometry.TriangleIndices.AddMany(qc, qn, pn);
            }

            return CreateVisual(geometry, snappedPrimitive);
        }

        private static Visual3D CreateCuboidView(Point3D Center, double Width, double Height, double Depth,
                                                 Vector3D W, Vector3D H, Vector3D D, SnappedPrimitive snappedPrimitive)
        {
            Point3D[] Pnts = new Point3D[8];
            Pnts[0] = new Point3D(-Width / 2, Height / 2, -Depth / 2);
            Pnts[1] = new Point3D(Width / 2, Height / 2, -Depth / 2);
            Pnts[2] = new Point3D(Width / 2, -Height / 2, -Depth / 2);
            Pnts[3] = new Point3D(-Width / 2, -Height / 2, -Depth / 2);
            Pnts[4] = new Point3D(-Width / 2, Height / 2, Depth / 2);
            Pnts[5] = new Point3D(Width / 2, Height / 2, Depth / 2);
            Pnts[6] = new Point3D(Width / 2, -Height / 2, Depth / 2);
            Pnts[7] = new Point3D(-Width / 2, -Height / 2, Depth / 2);
            double[][] P = FindTransformationMatrix(W, H, D);
            TransformPoints(Pnts, Center, P);

            Int32Collection Idx = new Int32Collection();
            Idx.AddMany(0, 2, 1);
            Idx.AddMany(0, 3, 2);
            Idx.AddMany(6, 5, 2);
            Idx.AddMany(2, 5, 1);
            Idx.AddMany(4, 5, 6);
            Idx.AddMany(4, 6, 7);
            Idx.AddMany(4, 7, 3);
            Idx.AddMany(4, 3, 0);
            Idx.AddMany(5, 4, 0);
            Idx.AddMany(5, 0, 1);
            Idx.AddMany(7, 6, 3);
            Idx.AddMany(2, 3, 6);

            var geometry = new MeshGeometry3D();
            geometry.Positions = new Point3DCollection(Pnts);
            geometry.TriangleIndices = Idx;

            return CreateVisual(geometry, snappedPrimitive, false);
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
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                    Debug.Write(G[j][i] + " ");
                Debug.WriteLine("");
            }
            double[][] P = new double[3][];
            for (int i = 0; i < 3; i++) P[i] = new double[3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    P[i][j] = G[i][j + 3];
            return P;
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
    }
}
