using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    class NewSphereViewModel : NewPrimitiveViewModel
    {
        public const double MIN_RADIUS = 0.005;

        private NewSphere model;
        private bool isUpdating;

        public NewSphereViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            radius = 0.1;
            model = new NewSphere { Radius = radius };
        }

        public void Init(NewSphere newSphere)
        {
            Contract.Requires(newSphere != null);
            Contract.Requires(newSphere.Radius > 0);

            Model = model = newSphere;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            isUpdating = true;
            try
            {
                Center = model.Center;
                Radius = model.Radius;
            }
            finally
            {
                isUpdating = false;
            }
        }

        #region Radius property

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RaisePropertyChanged(() => Radius);
                if (!isUpdating)
                    model.Radius = value;
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
    }
}
