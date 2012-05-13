using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using Petzold.Media3D;
using System.Windows;
using System.Diagnostics.Contracts;
using Utils;
using System.ComponentModel;
using SketchModeller.Infrastructure;
using SketchModeller.Utilities;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
namespace SketchModeller.Modelling.Views
{
    public class NewCuboidView : BaseNewPrimitiveView
    {
        private readonly NewCuboidViewModel viewModel;
        private readonly Cuboid cuboid;
        private readonly ChangeBasisTransform CBT;

        public NewCuboidView(NewCuboidViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            this.cuboid = new Cuboid();
            Children.Add(cuboid);

            cuboid.Bind(Cuboid.OriginProperty,
                () => viewModel.Center, () => viewModel.Width, () => viewModel.Height, () => viewModel.Depth,
                (center, width, height, depth) => new Point3D(-0.5 * width, -0.5 * height, -0.5 * depth));
            cuboid.Bind(Cuboid.DepthProperty, () => viewModel.Depth);
            cuboid.Bind(Cuboid.WidthProperty, () => viewModel.Width);
            cuboid.Bind(Cuboid.HeightProperty, () => viewModel.Height);

            CBT = new ChangeBasisTransform();

            CBT.Bind(ChangeBasisTransform.CenterProperty, () => viewModel.Center);
            CBT.Bind(ChangeBasisTransform.HeightVectorProperty, () => viewModel.H);
            CBT.Bind(ChangeBasisTransform.WidthVectorProperty, () => viewModel.W);
            CBT.Bind(ChangeBasisTransform.DepthVectorProperty, () => viewModel.D);

            AlgorithmicTransformCollection transformCollection = new AlgorithmicTransformCollection();
            transformCollection.Add(CBT);
            //transformCollection.Freeze();

            cuboid.AlgorithmicTransforms = transformCollection;
            cuboid.SetMaterials(GetDefaultFrontAndBackMaterials(viewModel));
        }
    }
}
