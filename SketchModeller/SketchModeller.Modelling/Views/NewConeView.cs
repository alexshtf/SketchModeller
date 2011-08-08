using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using System.Windows;
using System.ComponentModel;
using Utils;
using SketchModeller.Utilities;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using Petzold.Media3D;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.Views
{
    public class NewConeView : BaseNewPrimitiveView
    {
        private readonly NewConeViewModel viewModel;
        private readonly Cylinder cylinder;

        [InjectionConstructor]
        public NewConeView(NewConeViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;

            cylinder = new Cylinder();
            Children.Add(cylinder);

            cylinder.Bind(Cylinder.Radius1Property, () => viewModel.TopRadius);
            cylinder.Bind(Cylinder.Radius2Property, () => viewModel.BottomRadius);
            cylinder.Bind(Cylinder.Point1Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center + 0.5 * length * axis);
            cylinder.Bind(Cylinder.Point2Property,
                () => viewModel.Center,
                () => viewModel.Axis,
                () => viewModel.Length,
                (center, axis, length) => center - 0.5 * length * axis);

            cylinder.SetMaterials(GetDefaultFrontAndBackMaterials(viewModel));
        }

        public override void DragStart(Point startPos, LineRange startRay)
        {
            base.DragStart(startPos, startRay);
            viewModel.DragStartProximity = GetDragStartProximity(startRay);
        }

        protected override Vector3D ApproximateAxis
        {
            get { return viewModel.Axis; }
        }

        
        private NewConeViewModel.DragStartProximities GetDragStartProximity(LineRange startRay)
        {
            NewConeViewModel.DragStartProximities result = default(NewConeViewModel.DragStartProximities);
            bool success = false;

            var htParams = new RayHitTestParameters(startRay.Point1, startRay.Point2 - startRay.Point1);
            var topNode = this.VisualPathUp().TakeWhile(x => x is Visual3D).OfType<Visual3D>().Last();

            VisualTreeHelper.HitTest(
                topNode,
                null,
                htResult =>
                {
                    if (htResult.VisualHit.VisualPathUp().Contains(cylinder))
                    {
                        var htResult3d = htResult as RayMeshGeometry3DHitTestResult;
                        var topPlane = Plane3D.FromPointAndNormal(viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis, viewModel.Axis);
                        var botPlane = Plane3D.FromPointAndNormal(viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis, viewModel.Axis);

                        var topDist = topPlane.DistanceFromPoint(htResult3d.PointHit);
                        var botDist = botPlane.DistanceFromPoint(htResult3d.PointHit);

                        if (topDist < botDist)
                            result = NewConeViewModel.DragStartProximities.Top;
                        else
                            result = NewConeViewModel.DragStartProximities.Bottom;

                        success = true;
                        return HitTestResultBehavior.Stop;
                    }
                    else
                        return HitTestResultBehavior.Continue;
                },
                htParams);

            Debug.Assert(success == true);
            return result;
        }

    }
}
