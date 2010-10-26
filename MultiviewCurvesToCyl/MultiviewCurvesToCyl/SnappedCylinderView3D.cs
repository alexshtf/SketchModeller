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

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderView3D : IVisual3DFactory
    {
        public static readonly SnappedCylinderView3D Instance = new SnappedCylinderView3D();

        public Visual3D Create(object dataItem)
        {
            var result = new ModelVisual3D();
            dataItem.MatchClass<SnappedCylinderViewModel>(viewModel =>
                {
                    var model = new GeometryModel3D();
                    result.Content = model;
                    var helper = new GeometryModel3DHelper(result, model, viewModel);
                    GeometryModel3DHelper.SetInstance(result, helper);
                });
            return result;
        }

        private class GeometryModel3DHelper
        {
            private ModelVisual3D father;
            private GeometryModel3D view;
            private SnappedCylinderViewModel viewModel;

            public GeometryModel3DHelper(ModelVisual3D father, GeometryModel3D view, SnappedCylinderViewModel viewModel)
            {
                // assign fields
                this.father = father;
                this.view = view;
                this.viewModel = viewModel;

                // perform initialization
                var triangleIndicesSequence = viewModel.CylinderData.TriangleIndices.SelectMany(x => new int[] { x.Item1, x.Item2, x.Item3 });
                view.Geometry = new MeshGeometry3D
                {
                    Positions = new Point3DCollection(viewModel.CylinderData.Positions),
                    Normals = new Vector3DCollection(viewModel.CylinderData.Normals),
                    TriangleIndices = new Int32Collection(triangleIndicesSequence),
                };

                view.Material = new DiffuseMaterial
                {
                    Brush = new SolidColorBrush { Color = Colors.White },
                };
                view.BackMaterial = new DiffuseMaterial
                {
                    Brush = new SolidColorBrush { Color = Colors.Red },
                };

                // register for event changes
                viewModel.PositionUpdated += OnViewModelPositionUpdated;
                viewModel.NormalUpdated += OnViewModelNormalUpdated;
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                e.Match(() => viewModel.IsInWireframeMode, () =>
                    {
                        if (viewModel.IsInWireframeMode)
                            father.Content = null;
                        else
                            father.Content = view;
                    });
            }

            private void OnViewModelPositionUpdated(object sender, IndexedAttributeUpdateEventArgs e)
            {
                MeshGeometry.Positions[e.Index] = viewModel.CylinderData.Positions[e.Index];
            }

            private void OnViewModelNormalUpdated(object sender, IndexedAttributeUpdateEventArgs e)
            {
                MeshGeometry.Normals[e.Index] = viewModel.CylinderData.Normals[e.Index];
            }

            private MeshGeometry3D MeshGeometry
            {
                get { return (MeshGeometry3D)view.Geometry; }
            }

            #region Instance attached property
            
            public static readonly DependencyProperty InstanceProperty =
                DependencyProperty.RegisterAttached("Instance", typeof(GeometryModel3DHelper), typeof(GeometryModel3DHelper));

            public static void SetInstance(Visual3D target, GeometryModel3DHelper value)
            {
                target.SetValue(InstanceProperty, value);
            }

            public static GeometryModel3DHelper GetInstance(Visual3D target)
            {
                return (GeometryModel3DHelper)target.GetValue(InstanceProperty);
            } 

            #endregion
        }
    }
}
