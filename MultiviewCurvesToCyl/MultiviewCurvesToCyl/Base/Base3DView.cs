using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiviewCurvesToCyl.Controls;
using System.Windows.Media.Media3D;
using Utils;
using System.Diagnostics.Contracts;
using System.Windows.Threading;
using System.ComponentModel;

namespace MultiviewCurvesToCyl.Base
{
    abstract class Base3DView<T> : IVisual3DFactory
        where T : class, I3DViewModel
    {
        public Visual3D Create(object item)
        {
            var root = new ModelVisual3D();
            item.MatchClass<T>(viewModel =>
                {
                    var solidVisual = CreateSolidVisual(viewModel);
                    var wireframeVisual = CreateWireframeVisual(viewModel);
                    var helper = new Base3DViewHelper(viewModel, root, solidVisual, wireframeVisual);
                });
            return root;
        }

        protected abstract Visual3D CreateWireframeVisual(T viewModel);
        protected abstract Visual3D CreateSolidVisual(T viewModel);

        #region Base3DViewHelper class

        private class Base3DViewHelper : DispatcherObject
        {
            private I3DViewModel viewModel;
            private ModelVisual3D root;
            private Visual3D solidVisual;
            private Visual3D wireframeVisual;

            public Base3DViewHelper(I3DViewModel viewModel, ModelVisual3D root, Visual3D solidVisual, Visual3D wireframeVisual)
            {
                // member initialization
                this.viewModel = viewModel;
                this.root = root;
                this.solidVisual = solidVisual;
                this.wireframeVisual = wireframeVisual;

                if (viewModel.IsInWireframeMode)
                    root.Children.Add(wireframeVisual);
                else
                    root.Children.Add(solidVisual);

                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                e.Match(() => viewModel.IsInWireframeMode, () =>
                    {
                        if (viewModel.IsInWireframeMode)
                        {
                            root.Children.Remove(solidVisual);
                            if (!root.Children.Contains(wireframeVisual))
                                root.Children.Add(wireframeVisual);
                        }
                        else
                        {
                            root.Children.Remove(wireframeVisual);
                            if (!root.Children.Contains(solidVisual))
                                root.Children.Add(solidVisual);
                        }
                    });
            }

        }

        #endregion
    }
}
