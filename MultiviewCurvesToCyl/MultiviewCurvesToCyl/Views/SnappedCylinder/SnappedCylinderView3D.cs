using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using System.Windows.Media;
using System.Windows;
using MultiviewCurvesToCyl.MeshGeneration;
using System.ComponentModel;
using MultiviewCurvesToCyl.Controls;
using MultiviewCurvesToCyl.Base;
using Petzold.Media3D;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderView3D : Base3DView<SnappedCylinderViewModel>
    {
        public static SnappedCylinderView3D Instance = new SnappedCylinderView3D();

        protected override Visual3D CreateWireframeVisual(SnappedCylinderViewModel viewModel)
        {
            var visual = new WireFrame();
            var triangleIndicesSequence = viewModel.CylinderData.TriangleIndices.SelectMany(x => new int[] { x.Item1, x.Item2, x.Item3 });
            visual.Positions = new Point3DCollection(viewModel.CylinderData.Positions);
            visual.Normals = new Vector3DCollection(viewModel.CylinderData.Normals);
            visual.TriangleIndices = new Int32Collection(triangleIndicesSequence);
            visual.Color = Colors.Red;

            viewModel.PositionUpdated += (sender, args) => visual.Positions[args.Index] = viewModel.CylinderData.Positions[args.Index];
            viewModel.NormalUpdated += (sender, args) => visual.Normals[args.Index] = viewModel.CylinderData.Normals[args.Index];

            return visual;
        }

        protected override Visual3D CreateSolidVisual(SnappedCylinderViewModel viewModel)
        {
            var visual = new ModelVisual3D();
            var geometryModel = new GeometryModel3D();
            visual.Content = geometryModel;

            var triangleIndicesSequence = viewModel.CylinderData.TriangleIndices.SelectMany(x => new int[] { x.Item1, x.Item2, x.Item3 });
            var geometry = new MeshGeometry3D
            {
                Positions = new Point3DCollection(viewModel.CylinderData.Positions),
                Normals = new Vector3DCollection(viewModel.CylinderData.Normals),
                TriangleIndices = new Int32Collection(triangleIndicesSequence),
            };
            geometryModel.Geometry = geometry;

            geometryModel.Material = new DiffuseMaterial
            {
                Brush = new SolidColorBrush { Color = Colors.White },
            };
            geometryModel.BackMaterial = new DiffuseMaterial
            {
                Brush = new SolidColorBrush { Color = Colors.Red },
            };

            viewModel.PositionUpdated += (sender, args) => geometry.Positions[args.Index] = viewModel.CylinderData.Positions[args.Index];
            viewModel.NormalUpdated += (sender, args) => geometry.Normals[args.Index] = viewModel.CylinderData.Normals[args.Index];

            return visual;
        }
    }
}
