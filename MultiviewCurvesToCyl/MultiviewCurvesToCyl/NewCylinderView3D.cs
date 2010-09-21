using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace MultiviewCurvesToCyl
{
    class NewCylinderView3D : IModelFactory
    {
        public static readonly NewCylinderView3D Instance = new NewCylinderView3D();

        private const int SKELETON_POINTS = 50;
        private const int CYLINDER_SLICES = 50;

        public Model3D Create(object dataItem)
        {
            var result = new GeometryModel3D();
            dataItem.MatchClass<NewCylinderViewModel>(viewModel =>
                {
                    var skeleton = GenerateCylinderSkeleton(viewModel, SKELETON_POINTS);
                    var meshData = SkeletonToMesh.SkeletonToCylinder(skeleton, CYLINDER_SLICES);

                    result.Geometry = new MeshGeometry3D
                    {
                        Positions = new Point3DCollection(meshData.Item1),
                        Normals = new Vector3DCollection(meshData.Item2),
                        TriangleIndices = new Int32Collection(meshData.Item3),
                    };
                    result.Material = new DiffuseMaterial
                    {
                        Brush = new SolidColorBrush { Color = Colors.White },
                    };
                    result.BackMaterial = new DiffuseMaterial
                    {
                        Brush = new SolidColorBrush { Color = Colors.Red },
                    };

                    var lengthRadiusScaleTransform = new ScaleTransform3D();
                    lengthRadiusScaleTransform.Bind(ScaleTransform3D.ScaleXProperty, "Length", viewModel);
                    lengthRadiusScaleTransform.Bind(ScaleTransform3D.ScaleYProperty, "Radius", viewModel);
                    lengthRadiusScaleTransform.ScaleZ = 1.0;

                    var translateTransform = new TranslateTransform3D();
                    translateTransform.Bind(TranslateTransform3D.OffsetXProperty, "Center.X", viewModel);
                    translateTransform.Bind(TranslateTransform3D.OffsetYProperty, "Center.Y", viewModel);
                    translateTransform.Bind(TranslateTransform3D.OffsetZProperty, "Center.Z", viewModel);

                    var orientationTransform = new RotateTransform3D();
                    orientationTransform.Rotation = new AxisAngleRotation3D();
                    orientationTransform.Rotation.Bind(AxisAngleRotation3D.AxisProperty, "Orientation", viewModel, OrientationToAxisConverter.Instance);
                    orientationTransform.Rotation.Bind(AxisAngleRotation3D.AngleProperty, "Orientation", viewModel, OrientationToAngleConverter.Instance);

                    result.Transform = new Transform3DGroup
                    {
                        Children = new Transform3DCollection { lengthRadiusScaleTransform, orientationTransform, translateTransform },
                    };
                });

            return result;
        }

        private IEnumerable<SkeletonPoint> GenerateCylinderSkeleton(NewCylinderViewModel viewModel, int points)
        {
            var center = viewModel.Center;
            var orientation = new Vector3D(1, 0, 0);
            var start = MathUtils3D.Origin - orientation * 0.5;

            for (int i = 0; i < points; i++)
            {
                var t = i / ((double)points - 1);
                yield return new SkeletonPoint
                {
                    Position = start + t * orientation,
                    Normal   = orientation,
                    Radius   = 0.5,
                };
            }
        }

        #region OrientationToAxisConverter class

        [ValueConversion(typeof(Vector3D), typeof(Vector3D))]
        private class OrientationToAxisConverter : IValueConverter
        {
            public static readonly OrientationToAxisConverter Instance = new OrientationToAxisConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Vector3D)
                {
                    var concrete = (Vector3D)value;
                    var axis = Vector3D.CrossProduct(MathUtils3D.UnitX, concrete);
                    return axis;
                }
                else
                    return Binding.DoNothing;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException("One way converters don't support ConvertBack.");
            }
        }

        #endregion

        #region OrientationToAngleConverter class

        [ValueConversion(typeof(Vector3D), typeof(double))]
        private class OrientationToAngleConverter : IValueConverter
        {
            public static readonly OrientationToAngleConverter Instance = new OrientationToAngleConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Vector3D)
                {
                    var concrete = (Vector3D)value;
                    var angle = Vector3D.AngleBetween(concrete, MathUtils3D.UnitX);
                    return angle;
                }
                else
                    return Binding.DoNothing;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException("One way converters don't support ConvertBack.");
            }
        }

        #endregion
    }
}
