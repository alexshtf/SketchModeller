using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;
using SketchModeller.Utilities;

using WpfPoint3D = System.Windows.Media.Media3D.Point3D;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Commands;
using CollectionUtils = Utils.CollectionUtils;
using MathUtils3D = Utils.MathUtils3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        private const double MIN_LENGTH = 0.01;
        private const double MIN_DIAMETER = 0.01;

        private NewCylinder model;

        [InjectionConstructor]
        public NewCylinderViewModel(UiState uiState = null)
            : base(uiState)
        {
            // set default data
            diameter = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            model = new NewCylinder
            {
                Axis = axis,
                Length = length,
                Diameter = diameter,
            };
        }

        public void Initialize(NewCylinder newCylinder)
        {
            Contract.Requires(newCylinder != null);
            Contract.Requires(newCylinder.Axis != MathUtils3D.ZeroVector);
            Contract.Requires(newCylinder.Length > 0);
            Contract.Requires(newCylinder.Diameter > 0);

            Center = newCylinder.Center;
            Axis = newCylinder.Axis;
            Length = newCylinder.Length;
            Diameter = newCylinder.Diameter;
            Model = model = newCylinder;
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
                model.Axis = value;
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
                model.Center = value;
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
