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

namespace SketchModeller.Modelling.Views
{
    public class NewConeView : BaseNewPrimitiveView
    {
        private readonly NewConeViewModel viewModel;

        [InjectionConstructor]
        public NewConeView(NewConeViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            UpdateTranslation();
            UpdateAxis();
            UpdateMeshGeometry();
        }

        protected override void MovePosition(Vector3D moveVector)
        {
            viewModel.Center = viewModel.Center + moveVector;
        }

        protected override void Edit(int sign)
        {
            viewModel.Edit(sign);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            e.Match(() => viewModel.Center, UpdateTranslation);
            e.Match(() => viewModel.Length, UpdateLength);
            e.Match(() => viewModel.TopRadius, UpdateTopRadius);
            e.Match(() => viewModel.BottomRadius, UpdateBottomRadius);
            e.Match(() => viewModel.Axis, UpdateAxis);
        }

        #region view model change response methods

        private void UpdateTranslation()
        {
            UpdateTranslation(viewModel.Center);
        }

        private void UpdateLength()
        {
            UpdateMeshGeometry();
        }

        private void UpdateTopRadius()
        {
            UpdateMeshGeometry();
        }

        private void UpdateBottomRadius()
        {
            UpdateMeshGeometry();
        }

        private void UpdateAxis()
        {
            var rotationAxis = Vector3D.CrossProduct(MathUtils3D.UnitY, viewModel.Axis);
            var degrees = Vector3D.AngleBetween(MathUtils3D.UnitY, viewModel.Axis);
            UpdateRotation(rotationAxis, degrees);
        }

        #endregion

        private void UpdateMeshGeometry()
        {
            var topCenter = new Point3D(0, 0.5 * viewModel.Length, 0);
            var bottomCenter = new Point3D(0, -0.5 * viewModel.Length, 0);

            var topCircle = GenerateCircle(topCenter, viewModel.TopRadius);
            var bottomCircle = GenerateCircle(bottomCenter, viewModel.BottomRadius);

            Contract.Assume(topCircle.Length == bottomCircle.Length);
            var circlePtsCount = topCircle.Length;

            var meshGeometry = new MeshGeometry3D();
            meshGeometry.Positions.AddMany(topCircle);
            meshGeometry.Positions.AddMany(bottomCircle);
            meshGeometry.Positions.AddMany(topCenter, bottomCenter);

            var tcIndex = meshGeometry.Positions.Count - 2;
            var bcIndex = meshGeometry.Positions.Count - 1;

            // generate bottom circle triangles
            for (int i = 0; i < circlePtsCount; ++i)
            {
                var idx1 = i;
                var idx2 = (i + 1) % circlePtsCount;
                meshGeometry.TriangleIndices.AddMany(idx1, idx2, tcIndex);
            }

            // generate top circle triangles
            for (int i = 0; i < circlePtsCount; ++i)
            {
                var idx1 = circlePtsCount + i;
                var idx2 = circlePtsCount + (i + 1) % circlePtsCount;
                meshGeometry.TriangleIndices.AddMany(idx1, idx2, bcIndex);
            }

            // generate intermediate triangles
            for(int i = 0; i < circlePtsCount; ++i)
            {
                var topIdx1 = i;
                var topIdx2 = (i + 1) % circlePtsCount;
                var botIdx1 = circlePtsCount + i;
                var botIdx2 = circlePtsCount + (i + 1) % circlePtsCount;
                meshGeometry.TriangleIndices.AddMany(topIdx2, topIdx1, botIdx1);
                meshGeometry.TriangleIndices.AddMany(botIdx1, botIdx2, topIdx2);
            }

            meshGeometry.Freeze();
            UpdateGeometry(meshGeometry);
        }

        private Point3D[] GenerateCircle(Point3D center, double radius)
        {
            return ShapeHelper.GenerateCircle(center, MathUtils3D.UnitX, MathUtils3D.UnitZ, radius, 50);
        }
    }
}
