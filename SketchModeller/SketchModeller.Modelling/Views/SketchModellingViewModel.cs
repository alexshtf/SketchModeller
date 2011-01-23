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

using IFPoint3D = SketchModeller.Infrastructure.Data.Point3D;
using WPFPoint3D = System.Windows.Media.Media3D.Point3D;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure;
using System.Collections.Specialized;

namespace SketchModeller.Modelling.Views
{
    public class SketchModellingViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private SessionData sessionData;
        private IUnityContainer container;
        private ISketchCatalog sketchCatalog;
        private IEventAggregator eventAggregator;
        private ViewModelCollectionGenerator<NewPrimitiveViewModel> viewModelGenerator;

        public SketchModellingViewModel()
        {
            NewPrimitiveViewModels = new ObservableCollection<NewPrimitiveViewModel>();
            SnappedPrimitives = new ReadOnlyObservableCollection<SnappedPrimitive>(new ObservableCollection<SnappedPrimitive>());
            sketchPlane = SketchPlane.Default;
        }

        [InjectionConstructor]
        public SketchModellingViewModel(
            UiState uiState, 
            SessionData sessionData, 
            IEventAggregator eventAggregator, 
            IUnityContainer container,
            ISketchCatalog sketchCatalog)
            : this()
        {
            this.uiState = uiState;
            this.sessionData = sessionData;
            this.container = container;
            this.sketchCatalog = sketchCatalog;
            this.eventAggregator = eventAggregator;

            uiState.AddListener(this, () => uiState.SketchPlane);
            sessionData.AddListener(this, () => sessionData.SketchData);
            eventAggregator.GetEvent<SketchClickEvent>().Subscribe(OnSketchClick);

            sketchPlane = uiState.SketchPlane;

            viewModelGenerator = new ViewModelCollectionGenerator<NewPrimitiveViewModel>(
                NewPrimitiveViewModels, 
                sessionData.NewPrimitives, 
                NewPrimitiveDataToNewPrimitiveViewModel);

            SnappedPrimitives = new ReadOnlyObservableCollection<SnappedPrimitive>(sessionData.SnappedPrimitives);
        }

        public ObservableCollection<NewPrimitiveViewModel> NewPrimitiveViewModels { get; private set; }

        public ReadOnlyObservableCollection<SnappedPrimitive> SnappedPrimitives { get; private set; }

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

        public void Delete(NewPrimitiveViewModel newPrimitiveViewModel)
        {
            var model = newPrimitiveViewModel.Model;
            sessionData.NewPrimitives.Remove(model);
        }

        private NewPrimitiveViewModel NewPrimitiveDataToNewPrimitiveViewModel(object data)
        {
            NewPrimitiveViewModel result = null;
            data.MatchClass<NewCylinder>(cylinder =>
                {
                    var viewModel = container.Resolve<NewCylinderViewModel>();
                    viewModel.Initialize(cylinder);
                    result = viewModel;
                });

            if (result != null)
                result.Model = data;
            return result;
        }

        private void OnSketchClick(SketchClickInfo info)
        {
            if (uiState.Tool == Tool.InsertCylinder)
            {
                var point3d = GetClickPoint(info);
                var cylinderData = new NewCylinder
                {
                    Center = point3d.ToDataPoint(),
                    Axis = sketchPlane.YAxis.ToDataPoint(),
                    Diameter = 0.1,
                    Length = 0.2,
                };
                sessionData.NewPrimitives.Add(cylinderData);
            }
            uiState.Tool = Tool.Manipulation;

        }

        private void ResetModellingObjects(SketchData sketchData)
        {
            NewPrimitiveViewModels.Clear();
            if (sketchData.Cylinders != null)
            {
                foreach (var newCylinder in sketchData.Cylinders)
                {
                    var viewModel = container.Resolve<NewCylinderViewModel>();
                    viewModel.Initialize(newCylinder);
                    NewPrimitiveViewModels.Add(viewModel);
                }
            }
        }

        private System.Windows.Media.Media3D.Point3D GetClickPoint(SketchClickInfo info)
        {
            var plane = Plane3D.FromPointAndNormal(uiState.SketchPlane.Center, uiState.SketchPlane.Normal);
            var t = plane.IntersectLine(info.RayStart, info.RayEnd);
            
            Debug.Assert(!double.IsNaN(t) && !double.IsInfinity(t), "Intersection point must exist. We on purpose orient the camera towards the sketch plane");
            Debug.Assert(t >= 0, "Intersection point must be on the ray, because we on purpose orient the camera towards the sketch plane");

            return MathUtils3D.Lerp(info.RayStart, info.RayEnd, t);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                var eventArgs = (PropertyChangedEventArgs)e;
                if (eventArgs.Match(() => uiState.SketchPlane))
                    SketchPlane = uiState.SketchPlane;
                if (eventArgs.Match(() => sessionData.SketchData))
                    ResetModellingObjects(sessionData.SketchData);

                return true;
            }

            return false;
        }
    }
}
