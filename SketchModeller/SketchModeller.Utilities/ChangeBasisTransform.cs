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
            for (int i = 0; i < points.Count; i++)
            {
                Point3D Pt = new Point3D();
                Pt.X = points[i].X * W.X + points[i].Y * W.Y + points[i].Z * W.Z + Center.X;
                Pt.Y = points[i].X * H.X + points[i].Y * H.Y + points[i].Z * H.Z + Center.Y;
                Pt.Z = points[i].X * D.X + points[i].Y * D.Y + points[i].Z * D.Z + Center.Z;
                points[i] = Pt;
            }
        }
    }
}
