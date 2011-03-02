using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Modelling;
using Controls;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Logging;
using Petzold.Media3D;
using System.Diagnostics;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchModellingView.xaml
    /// </summary>
    public partial class SketchModellingView
    {
        private SketchModellingViewModel viewModel;
        private BaseNewPrimitiveView draggedPrimitive;

        public SketchModellingView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchModellingView(SketchModellingViewModel viewModel, ILoggerFacade logger)
            : this()
        {
            this.viewModel = viewModel;

            var displayOptions = viewModel.DisplayOptions;

            cloningVisual3D.ItemsSource = viewModel.NewPrimitiveViewModels;
            cloningVisual3D.Visual3DFactory = new Visual3DFactory(logger);
            cloningVisual3D.Bind(CloningVisual3D.IsVisibleProperty, () => displayOptions.IsTemporaryPrimitivesShown);

            snappedCloningVisual3d.ItemsSource = viewModel.SnappedPrimitives;
            snappedCloningVisual3d.Visual3DFactory = SketchModeller.Modelling.ModelViews.ModelViewerSnappedFactory.Instance;
            snappedCloningVisual3d.Bind(CloningVisual3D.IsVisibleProperty, () => displayOptions.IsSnappedPrimitivesShown);
        }

        class Visual3DFactory : IVisual3DFactory
        {
            private ILoggerFacade logger;

            public Visual3DFactory(ILoggerFacade logger)
            {
                this.logger = logger;
            }

            public Visual3D Create(object item)
            {
                Visual3D result = null;
                
                item.MatchClass<NewCylinderViewModel>(
                    viewModel => result = new NewCylinderView(viewModel, logger));
                item.MatchClass<NewHalfSphereViewModel>(
                    viewModel => result = new NewHalfSphereView(viewModel, logger));
                item.MatchClass<NewConeViewModel>(
                    viewModel => result = new NewConeView(viewModel, logger));

                Contract.Assume(result != null);
                return result;
            }
        }

        public void SelectPrimitive(LineRange lineRange)
        {
            var htParameters = new RayHitTestParameters(lineRange.Point1, lineRange.Point2 - lineRange.Point1);
            bool success = false;
            VisualTreeHelper.HitTest(this, 
                null, 
                htResult => 
                {
                    var primitiveHit = 
                        htResult.VisualHit
                        .VisualPathUp()
                        .OfType<BaseNewPrimitiveView>()
                        .FirstOrDefault();

                    if (primitiveHit != null)
                    {
                        draggedPrimitive = primitiveHit;
                        draggedPrimitive.DragStart(lineRange);
                        viewModel.SelectPrimitive(((INewPrimitiveView)draggedPrimitive).ViewModel);
                        success = true;
                        return HitTestResultBehavior.Stop;
                    }
                    else
                        return HitTestResultBehavior.Continue;
                }, 
                htParameters);

            if (!success)
                viewModel.UnselectAllPrimitives();
        }

        public void DragPrimitive(LineRange lineRange)
        {
            if (draggedPrimitive != null)
                draggedPrimitive.Drag(lineRange);
        }

        public void EndDrag()
        {
            if (draggedPrimitive != null)
                draggedPrimitive.DragEnd();
            draggedPrimitive = null;
        }
    }
}
