using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Commands;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Modelling.Views
{
    public class NewConeViewModel : NewPrimitiveViewModel
    {
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private NewCone model;
        private bool initializing;

        [InjectionConstructor]
        public NewConeViewModel(UiState uiState = null, SessionData sessionData = null, IEventAggregator eventAggregator = null)
            : base(uiState, sessionData, eventAggregator)
        {
            topRadius = 0.2;
            bottomRadius = 0.2;
            length = 0.5;
            axis = MathUtils3D.UnitZ;
            center = MathUtils3D.Origin;

            model = new NewCone();
            UpdateModel();
        }

        public void Init(NewCone newModel)
        {
            this.model = newModel;
            UpdateFromModel();
        }

        public override void UpdateFromModel()
        {
            initializing = true;
            try
            {
                Axis = model.Axis;
                Center = model.Center;
                Length = model.Length;
                TopRadius = model.TopRadius;
                BottomRadius = model.BottomRadius;
            }
            finally
            {
                initializing = false;
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

        #region TopRadius property

        private double topRadius;

        public double TopRadius
        {
            get { return topRadius; }
            set
            {
                topRadius = value;
                RaisePropertyChanged(() => TopRadius);
            }
        }

        #endregion

        #region BottomRadius property

        private double bottomRadius;

        public double BottomRadius
        {
            get { return bottomRadius; }
            set
            {
                bottomRadius = value;
                RaisePropertyChanged(() => BottomRadius);
            }
        }

        #endregion

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            UpdateModel();
        }

        private void UpdateModel()
        {
            if (!initializing)
            {
                model.Axis = axis;
                model.Center = center;
                model.Length = length;
                model.TopRadius = topRadius;
                model.BottomRadius = bottomRadius;
            }
        }
    }
}
