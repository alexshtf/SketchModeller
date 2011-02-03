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

namespace SketchModeller.Modelling.Views
{
    class NewCylinderView : BaseNewPrimitiveView, IWeakEventListener
    {
        private NewCylinderViewModel viewModel;
        private HollowCylinderMesh cylinderMesh;

        public NewCylinderView(NewCylinderViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;
            cylinderMesh = new HollowCylinderMesh();

            viewModel.AddListener(this, () => viewModel.Center);
            viewModel.AddListener(this, () => viewModel.Diameter);
            viewModel.AddListener(this, () => viewModel.Length);
            viewModel.AddListener(this, () => viewModel.Axis);

            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            cylinderMesh.Length = viewModel.Length;
            UpdateMeshGeometry();
            UpdateTranslation();
            UpdateRotation();
        }


        protected override void MovePosition(Vector3D moveVector)
        {
            viewModel.Center = viewModel.Center + moveVector;
        }

        protected override void Edit(int sign)
        {
            viewModel.Edit(sign);
        }

        private void UpdateMeshGeometry()
        {
            var geometry = cylinderMesh.Geometry.Clone() as MeshGeometry3D;
            Contract.Assume(geometry != null, "Geometry must be a MeshGeometry3D");
            UpdateGeometry(geometry);
        }

        private void UpdateCylinderRadius()
        {
            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            UpdateMeshGeometry();
        }

        private void UpdateCylinderLength()
        {
            cylinderMesh.Length = viewModel.Length;
            UpdateMeshGeometry();
        }

        private void UpdateTranslation()
        {
            var newPosition = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis.Normalized();
            UpdateTranslation(newPosition);
        }

        private void UpdateRotation()
        {
            var rotationAxis = Vector3D.CrossProduct(MathUtils3D.UnitY, viewModel.Axis);
            var degrees = Vector3D.AngleBetween(MathUtils3D.UnitY, viewModel.Axis);
            UpdateRotation(rotationAxis, degrees);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;

            eventArgs.Match(() => viewModel.Center, UpdateTranslation);
            eventArgs.Match(() => viewModel.Length, UpdateCylinderLength);
            eventArgs.Match(() => viewModel.Axis, UpdateRotation);
            eventArgs.Match(() => viewModel.Diameter, UpdateCylinderRadius);

            return true;
        }
    }
}
