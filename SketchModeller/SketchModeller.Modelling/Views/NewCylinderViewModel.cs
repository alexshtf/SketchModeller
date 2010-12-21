using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;

using WpfPoint3D = System.Windows.Media.Media3D.Point3D;

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        private readonly UiState uiState;
        private NewCylinder model;

        public NewCylinderViewModel()
        {
            diameter = 0.1;
            length = 0.2;
            axis = MathUtils3D.UnitZ;
        }

        [InjectionConstructor]
        public NewCylinderViewModel(UiState uiState)
            : this()
        {
            this.uiState = uiState;
            model = new NewCylinder();
        }

        public void Initialize(WpfPoint3D center, Vector3D axis)
        {
            Center = center;
            Axis = axis;
        }

        internal void Initialize(NewCylinder newCylinder)
        {
            Center = newCylinder.Center.ToWpfPoint();
            Axis = newCylinder.Axis.ToWpfVector();
            Length = newCylinder.Length;
            Diameter = newCylinder.Diameter;
            model = newCylinder;
        }

        public SketchPlane SketchPlane
        {
            get { return uiState.SketchPlane; }
        }

        #region Axis property

        private Vector3D axis;

        public Vector3D Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                RaisePropertyChanged(() => Axis);
                model.Axis = value.ToDataPoint();
            }
        }

        #endregion

        #region Diameter property

        private double diameter;

        public double Diameter
        {
            get { return diameter; }
            set
            {
                diameter = value;
                RaisePropertyChanged(() => Diameter);
                model.Diameter = value;
            }
        }

        #endregion

        #region Center property

        private WpfPoint3D center;

        public WpfPoint3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
                model.Center = value.ToDataPoint();
            }
        }

        #endregion

        #region Length property

        private double length;

        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
                model.Length = value;
            }
        }

        #endregion

    }
}
