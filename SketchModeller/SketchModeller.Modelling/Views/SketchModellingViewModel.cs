using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Unity;
using System.Windows;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using Utils;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Diagnostics;

namespace SketchModeller.Modelling.Views
{
    public class SketchModellingViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private IUnityContainer container;

        public SketchModellingViewModel()
        {
            NewPrimitiveViewModels = new ObservableCollection<NewPrimitiveViewModel>();
            sketchPlane = SketchPlane.Default;
        }

        [InjectionConstructor]
        public SketchModellingViewModel(UiState uiState, IEventAggregator eventAggregator, IUnityContainer container)
            : this()
        {
            this.uiState = uiState;
            this.container = container;

            uiState.AddListener(this, () => uiState.SketchPlane);
            eventAggregator.GetEvent<SketchClickEvent>().Subscribe(OnSketchClick);

            sketchPlane = uiState.SketchPlane;
        }

        public ObservableCollection<NewPrimitiveViewModel> NewPrimitiveViewModels { get; private set; }

        #region SketchPlane property

        private SketchPlane sketchPlane;

        public SketchPlane SketchPlane
        {
            get { return sketchPlane; }
            set
            {
                sketchPlane = value;
                RaisePropertyChanged(() => SketchPlane);
            }
        }

        #endregion

        public void OnSketchClick(SketchClickInfo info)
        {
            if (uiState.Tool == Tool.InsertCylinder)
            {
                var point3d = GetClickPoint(info); // TODO: Extract from info.
                var viewModel = container.Resolve<NewCylinderViewModel>();
                viewModel.Initialize(center: point3d, axis: sketchPlane.YAxis);
                NewPrimitiveViewModels.Add(viewModel);
            }
            uiState.Tool = Tool.Manipulation;

        }

        private Point3D GetClickPoint(SketchClickInfo info)
        {
            var sketchPlane = uiState.SketchPlane;

            // extract mathematicsl symbols from sketchplane / info
            var p0 = sketchPlane.Center;
            var n = sketchPlane.Normal;
            var l0 = info.RayStart;
            var l = info.RayEnd - info.RayStart;

            var t = Vector3D.DotProduct(p0 - l0, n) / Vector3D.DotProduct(l, n);
            
            Debug.Assert(!double.IsNaN(t) && !double.IsInfinity(t), "Intersection point must exist. We on purpose orient the camera towards the sketch plane");
            Debug.Assert(t >= 0, "Intersection point must be on the ray, because we on purpose orient the camera towards the sketch plane");

            return l0 + t * l;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;
             
            var eventArgs = (PropertyChangedEventArgs)e;
            if (eventArgs.Match(() => uiState.SketchPlane))
                SketchPlane = uiState.SketchPlane;

            return true;
        }

        public void Delete(NewPrimitiveViewModel newPrimitiveViewModel)
        {
            NewPrimitiveViewModels.Remove(newPrimitiveViewModel);
        }
    }
}
