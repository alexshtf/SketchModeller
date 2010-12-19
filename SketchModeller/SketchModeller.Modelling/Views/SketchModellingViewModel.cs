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

namespace SketchModeller.Modelling.Views
{
    public class SketchModellingViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private SessionData sessionData;
        private IUnityContainer container;
        private ISketchCatalog sketchCatalog;
        private IEventAggregator eventAggregator;

        public SketchModellingViewModel()
        {
            NewPrimitiveViewModels = new ObservableCollection<NewPrimitiveViewModel>();
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
            eventAggregator.GetEvent<SaveSketchEvent>().Subscribe(OnSaveSketch);

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

        public void OnSaveSketch(object dummy)
        {
            // synchronize modelling session changed back to SketchData
            sessionData.SketchData.Cylinders = 
                (from cylinderVM in NewPrimitiveViewModels.OfType<NewCylinderViewModel>()
                 select new NewCylinder
                 {
                     Axis = cylinderVM.Axis.ToDataPoint(),
                     Center = cylinderVM.Center.ToDataPoint(),
                     Diameter = cylinderVM.Diameter,
                     Length = cylinderVM.Length,
                 }).ToArray();

            // save the new SketchData to the relevant files
            Work.Execute(
                eventAggregator, 
                () => sketchCatalog.SaveSketchAsync(sessionData.SketchName, sessionData.SketchData));
        }

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
            if (managerType != typeof(PropertyChangedEventManager))
                return false;
             
            var eventArgs = (PropertyChangedEventArgs)e;
            if (eventArgs.Match(() => uiState.SketchPlane))
                SketchPlane = uiState.SketchPlane;
            if (eventArgs.Match(() => sessionData.SketchData))
                ResetModellingObjects(sessionData.SketchData);

            return true;
        }

        public void Delete(NewPrimitiveViewModel newPrimitiveViewModel)
        {
            NewPrimitiveViewModels.Remove(newPrimitiveViewModel);
        }
    }
}
