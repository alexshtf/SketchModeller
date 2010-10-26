using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Petzold.Media3D;
using Utils;
using System.Windows.Media;
using MultiviewCurvesToCyl.MeshGeneration;
using System.Windows;
using System.ComponentModel;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderWireframeView3D : IVisual3DFactory
    {
        public static readonly SnappedCylinderWireframeView3D Instance = new SnappedCylinderWireframeView3D();

        public Visual3D Create(object item)
        {
            var result = new ModelVisual3D();
            item.MatchClass<SnappedCylinderViewModel>(viewModel =>
                {
                    var wireFrame = new WireFrame();
                    var helper = new WireframeHelper(result, wireFrame, viewModel);
                    WireframeHelper.SetInstance(result, helper);
                });
            return result;
        }

        private class WireframeHelper
        {
            private readonly ModelVisual3D father;
            private readonly WireFrame view;
            private readonly SnappedCylinderViewModel viewModel;

            public WireframeHelper(ModelVisual3D father, WireFrame view, SnappedCylinderViewModel viewModel)
            {
                this.father = father;
                this.view = view;
                this.viewModel = viewModel;

                var triangleIndicesSequence = viewModel.CylinderData.TriangleIndices.SelectMany(x => new int[] { x.Item1, x.Item2, x.Item3 });
                view.Positions = new Point3DCollection(viewModel.CylinderData.Positions);
                view.Normals = new Vector3DCollection(viewModel.CylinderData.Normals);
                view.TriangleIndices = new Int32Collection(triangleIndicesSequence);

                view.Color = Colors.Red;

                viewModel.PositionUpdated += OnViewModelPositionUpdated;
                viewModel.NormalUpdated += OnViewModelNormalUpdated;
                viewModel.PropertyChanged += OnViewModelPropertyChanged;

                if (viewModel.IsInWireframeMode)
                    father.Children.Add(view);
            }

            void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                e.Match(() => viewModel.IsInWireframeMode, () =>
                    {
                        if (viewModel.IsInWireframeMode && !father.Children.Contains(view))
                            father.Children.Add(view);
                        else if (!viewModel.IsInWireframeMode)
                            father.Children.Remove(view);
                    });
            }

            private void OnViewModelPositionUpdated(object sender, IndexedAttributeUpdateEventArgs e)
            {
                view.Positions[e.Index] = viewModel.CylinderData.Positions[e.Index];
            }

            private void OnViewModelNormalUpdated(object sender, IndexedAttributeUpdateEventArgs e)
            {
                view.Normals[e.Index] = viewModel.CylinderData.Normals[e.Index];
            }

            #region Instance attached property

            public static readonly DependencyProperty InstanceProperty =
                DependencyProperty.RegisterAttached("Instance", typeof(WireframeHelper), typeof(WireframeHelper));

            public static void SetInstance(Visual3D target, WireframeHelper value)
            {
                target.SetValue(InstanceProperty, value);
            }

            public static WireframeHelper GetInstance(Visual3D target)
            {
                return (WireframeHelper)target.GetValue(InstanceProperty);
            }

            #endregion
        }
    }
}
