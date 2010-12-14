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

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        private readonly UiState uiState;

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
        }

        public void Initialize(Point3D center, Vector3D axis)
        {
            Center = center;
            Axis = axis;
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
            }
        }

        #endregion

        #region Center property

        private Point3D center;

        public Point3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
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
            }
        }

        #endregion
    }
}
