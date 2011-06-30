using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;
using Microsoft.Practices.Unity;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;

namespace SketchModeller.Modelling.Views
{
    class NewSGCViewModel : NewPrimitiveViewModel
    {
        private static readonly ReadOnlyCollection<ComponentViewModel> EMPTY_COMPONENTS =
            Array.AsReadOnly(new ComponentViewModel[0]);

        private NewStraightGenCylinder model;
        private bool isUpdating;

        [InjectionConstructor]
        public NewSGCViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            model = new NewStraightGenCylinder();
            components = EMPTY_COMPONENTS;
        }

        public void Init(NewStraightGenCylinder newSgc)
        {
            Model = model = newSgc;
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
                Components = GenerateComponentViewModels(model.Components);
            }
            finally
            {
                isUpdating = false;
            }
        }

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

        #region Components property

        private ReadOnlyCollection<ComponentViewModel> components;

        public ReadOnlyCollection<ComponentViewModel> Components
        {
            get { return components; }
            set
            {
                components = value;
                RaisePropertyChanged(() => Components);
                if (!isUpdating)
                    model.Components = GenerateComponents(value);
            }
        }

        #endregion
        
        #region component <--> component view models methods

        private static CylinderComponent[] GenerateComponents(IEnumerable<ComponentViewModel> cvms)
        {
            var resultQuery =
                from cvm in cvms
                select new CylinderComponent(cvm.Radius, cvm.Progress);

            return resultQuery.ToArray();
        }

        private static  ReadOnlyCollection<ComponentViewModel> GenerateComponentViewModels(IEnumerable<CylinderComponent> ccs)
        {
            var resultQuery =
                from cc in ccs
                select new ComponentViewModel(cc.Radius, cc.Progress);

            return Array.AsReadOnly(resultQuery.ToArray());
        }

        #endregion

        #region ComponentViewModel class

        public class ComponentViewModel
        {
            private readonly double radius;
            private readonly double progress;

            public ComponentViewModel(double radius, double progress)
            {
                this.radius = radius;
                this.progress = progress;
            }

            public double Radius
            {
                get { return radius; }
            }

            public double Progress
            {
                get { return progress; }
            }
        }

        #endregion
    }


}
