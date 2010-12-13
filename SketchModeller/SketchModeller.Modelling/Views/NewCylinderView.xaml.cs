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
using System.ComponentModel;
using Utils;
using Petzold.Media3D;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for NewCylinderView.xaml
    /// </summary>
    public partial class NewCylinderView : INewPrimitiveView, IWeakEventListener
    {
        private NewCylinderViewModel viewModel;
        private HollowCylinderMesh cylinderMesh;

        public NewCylinderView()
        {
            InitializeComponent();
            cylinderMesh = new HollowCylinderMesh();
        }

        public NewCylinderView(NewCylinderViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;

            viewModel.AddListener(this, () => viewModel.Center);
            viewModel.AddListener(this, () => viewModel.Diameter);
            viewModel.AddListener(this, () => viewModel.Length);
            viewModel.AddListener(this, () => viewModel.Axis);

            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            cylinderMesh.Length = viewModel.Length;
            geometry.Geometry = cylinderMesh.Geometry;
            UpdateTranslation();
            UpdateRotation();
        }

        private void UpdateMeshGeometry()
        {
            var geometry = cylinderMesh.Geometry.Clone() as MeshGeometry3D;
            Contract.Assume(geometry != null, "Geometry must be a MeshGeometry3D");
        }

        private void UpdateCylinderRadius()
        {
            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            geometry.Geometry = cylinderMesh.Geometry;
        }

        private void UpdateCylinderLength()
        {
            cylinderMesh.Length = viewModel.Length;
            geometry.Geometry = cylinderMesh.Geometry;
        }

        private void UpdateTranslation()
        {
            translation.OffsetX = viewModel.Center.X;
            translation.OffsetY = viewModel.Center.Y;
            translation.OffsetZ = viewModel.Center.Z;
        }

        private void UpdateRotation()
        {
            var rotationAxis = Vector3D.CrossProduct(MathUtils3D.UnitY, viewModel.Axis);
            var angle = Vector3D.AngleBetween(MathUtils3D.UnitY, viewModel.Axis);
            rotation.Axis = rotationAxis;
            rotation.Angle = angle;
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

        NewPrimitiveViewModel INewPrimitiveView.ViewModel
        {
            get { return viewModel; }
        }
    }
}
