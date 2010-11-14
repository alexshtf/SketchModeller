using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiviewCurvesToCyl.Base;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Petzold.Media3D;

namespace MultiviewCurvesToCyl
{
    class PointsBasedCylinderView3D : Base3DView<PointsBasedCylinderViewModel>
    {
        public static readonly PointsBasedCylinderView3D Instance = new PointsBasedCylinderView3D();

        protected override Visual3D CreateWireframeVisual(PointsBasedCylinderViewModel viewModel)
        {
            var visual = new WireFrame();
            visual.Color = Colors.Red;
            visual.Positions = new Point3DCollection(viewModel.Positions);
            visual.Normals = new Vector3DCollection(viewModel.Normals);
            visual.TriangleIndices = new Int32Collection(viewModel.TriangleIndices);

            return visual;
        }

        protected override Visual3D CreateSolidVisual(PointsBasedCylinderViewModel viewModel)
        {
            var visual = new ModelVisual3D();
            var geometryModel = new GeometryModel3D();
            visual.Content = geometryModel;

            var geometry = new MeshGeometry3D();
            geometry.Positions = new Point3DCollection(viewModel.Positions);
            geometry.Normals = new Vector3DCollection(viewModel.Normals);
            geometry.TriangleIndices = new Int32Collection(viewModel.TriangleIndices);
            
            geometryModel.Geometry = geometry;
            geometryModel.Material = new DiffuseMaterial
            {
                Brush = new SolidColorBrush { Color = Colors.White },
            };
            geometryModel.BackMaterial = new DiffuseMaterial
            {
                Brush = new SolidColorBrush { Color = Colors.Red },
            };

            return visual;
        }
    }
}
