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

using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Commands;
using CollectionUtils = Utils.CollectionUtils;
using MathUtils3D = Utils.MathUtils3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    public class NewCylinderViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private NewCylinder model;
        private bool isUpdating;

        [InjectionConstructor]
        public NewCylinderViewModel(UiState uiState = null, SessionData sessionData = null)
            : base(uiState, sessionData)
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

        public void Init(NewCylinder newCylinder)
        {
            Contract.Requires(newCylinder != null);
            Contract.Requires(newCylinder.Axis != MathUtils3D.ZeroVector);
            Contract.Requires(newCylinder.Length > 0);
            Contract.Requires(newCylinder.Diameter > 0);
            
            Model = model = newCylinder;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            isUpdating = true;
            try
            {
                Center = model.Center;
                Axis = model.Axis;
                Length = model.Length;
                Diameter = model.Diameter;
            }
            finally
            {
                isUpdating = false;
            }
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
                if (!isUpdating)
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
                if (!isUpdating)
                    model.Diameter = value;
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
                if (!isUpdating)
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
                if (!isUpdating)
                    model.Length = value;
            }
        }

        #endregion

    }
}
