using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Petzold.Media3D;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Utilities
{
    public class ChangeBasisTransform : AlgorithmicTransform
    {
        public ChangeBasisTransform()
        {
        }

        // Center property.
        // ----------------
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center",
                typeof(Point3D), typeof(ChangeBasisTransform),
                new PropertyMetadata(new Point3D(0, 0, 0)));

        public Point3D Center
        {
            set { SetValue(CenterProperty, value); }
            get { return (Point3D)GetValue(CenterProperty); }
        }

        // HeightVectorProperty property.
        // ----------------
        public static readonly DependencyProperty HeightVectorProperty =
            DependencyProperty.Register("H",
                typeof(Vector3D), typeof(ChangeBasisTransform),
                new PropertyMetadata(new Vector3D(0, 1, 0)));

        public Vector3D H
        {
            set { SetValue(HeightVectorProperty, value); }
            get { return (Vector3D)GetValue(HeightVectorProperty); }
        }

        // WidthVectorProperty property.
        // ----------------
        public static readonly DependencyProperty WidthVectorProperty =
            DependencyProperty.Register("W",
                typeof(Vector3D), typeof(ChangeBasisTransform),
                new PropertyMetadata(new Vector3D(1, 0, 0)));

        public Vector3D W
        {
            set { SetValue(WidthVectorProperty, value); }
            get { return (Vector3D)GetValue(WidthVectorProperty); }
        }

        // DeothVectorProperty property.
        // ----------------
        public static readonly DependencyProperty DepthVectorProperty =
            DependencyProperty.Register("D",
                typeof(Vector3D), typeof(ChangeBasisTransform),
                new PropertyMetadata(new Vector3D(0, 0, 1)));

        public Vector3D D
        {
            set { SetValue(DepthVectorProperty, value); }
            get { return (Vector3D)GetValue(DepthVectorProperty); }
        }


        protected override Freezable CreateInstanceCore()
        {
            return new ChangeBasisTransform();
        }

        public override void Transform(Point3DCollection points)
        {
            //It accepts a primitive centered at the center of the coordinate System.
            double[][] P = FindTransformationMatrix(W, H, D);
            for (int i = 0; i < points.Count; i++)
            {
                Point3D Pt = new Point3D();
                Pt.X = points[i].X * P[0][0] + points[i].Y * P[1][0] + points[i].Z * P[2][0] + Center.X;
                Pt.Y = points[i].X * P[0][1] + points[i].Y * P[1][1] + points[i].Z * P[2][1] + Center.Y;
                Pt.Z = points[i].X * P[0][2] + points[i].Y * P[1][2] + points[i].Z * P[2][2] + Center.Z;
                points[i] = Pt;
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
    }
}
