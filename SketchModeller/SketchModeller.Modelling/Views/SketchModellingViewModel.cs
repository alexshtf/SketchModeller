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
using SketchModeller.Utilities;

using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure;
using System.Collections.Specialized;

using SnappedPrimitivesCollection = SketchModeller.Modelling.ModelViews.ModelViewerViewModel.SnappedPrimitivesCollection;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Views
{
    public class SketchModellingViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private SessionData sessionData;
        private IUnityContainer container;
        private ISketchCatalog sketchCatalog;
        private IEventAggregator eventAggregator;
        private ViewModelCollectionGenerator<NewPrimitiveViewModel, NewPrimitive> viewModelGenerator;

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
            DisplayOptions displayOptions,
            IEventAggregator eventAggregator, 
            IUnityContainer container,
            ISketchCatalog sketchCatalog)
            : this()
        {
            this.uiState = uiState;
            this.sessionData = sessionData;
            this.DisplayOptions = displayOptions;
            this.container = container;
            this.sketchCatalog = sketchCatalog;
            this.eventAggregator = eventAggregator;

            uiState.AddListener(this, () => uiState.SketchPlane);
            eventAggregator.GetEvent<SketchClickEvent>().Subscribe(OnSketchClick);
            eventAggregator.GetEvent<SnapCompleteEvent>().Subscribe(OnSnapComplete);

            sketchPlane = uiState.SketchPlane;

            viewModelGenerator = new ViewModelCollectionGenerator<NewPrimitiveViewModel, NewPrimitive>(
                NewPrimitiveViewModels, 
                sessionData.NewPrimitives, 
                NewPrimitiveDataToNewPrimitiveViewModel);

            SnappedPrimitives = new SnappedPrimitivesCollection(sessionData.SnappedPrimitives);
        }

        public DisplayOptions DisplayOptions { get; private set; }

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

        public void SelectPrimitive(NewPrimitiveViewModel newPrimitiveViewModel)
        {
            var toUnSelect = sessionData.SelectedNewPrimitives.ToArray();
            foreach (var item in toUnSelect)
                if (item != newPrimitiveViewModel.Model)
                    item.IsSelected = false;

            newPrimitiveViewModel.Model.IsSelected = true;
        }

        public void UnselectAllPrimitives()
        {
            var toUnSelect = sessionData.SelectedNewPrimitives.ToArray();
            foreach (var item in toUnSelect)
                item.IsSelected = false;
        }

        private NewPrimitiveViewModel NewPrimitiveDataToNewPrimitiveViewModel(NewPrimitive data)
        {
            NewPrimitiveViewModel result = null;
            data.MatchClass<NewCylinder>(cylinder =>
                {
                    var viewModel = container.Resolve<NewCylinderViewModel>();
                    viewModel.Initialize(cylinder);
                    result = viewModel;
                });
            data.MatchClass<NewHalfSphere>(halfSphere =>
                {
                    var viewModel = container.Resolve<NewHalfSphereViewModel>();
                    viewModel.Initialize(halfSphere);
                    result = viewModel;
                });
            data.MatchClass<NewCone>(newCone =>
                {
                    var viewModel = container.Resolve<NewConeViewModel>();
                    viewModel.Init(newCone);
                    result = viewModel;
                });

            if (result != null)
                result.Model = data;
            return result;
        }

        private void OnSnapComplete(object payload)
        {
            ((SnappedPrimitivesCollection)SnappedPrimitives).RaiseReset();
        }

        private void OnSketchClick(SketchClickInfo info)
        {
            if (uiState.Tool == Tool.Duplicate)
            {
                var selectedNewPrimitive = sessionData.SelectedNewPrimitives.FirstOrDefault();
                if (selectedNewPrimitive != null)
                {
                    var point3d = GetClickPoint(info);
                    var duplicate = Duplicate(selectedNewPrimitive, point3d);
                    sessionData.NewPrimitives.Add(duplicate);
                }
            }
            uiState.Tool = Tool.Manipulation;
        }

        private NewPrimitive Duplicate(NewPrimitive selectedNewPrimitive, Point3D point3d)
        {
            Contract.Requires(selectedNewPrimitive != null);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            Contract.Ensures(Contract.Result<NewPrimitive>().GetType() == selectedNewPrimitive.GetType());

            NewPrimitive duplicate = null;
            selectedNewPrimitive.MatchClass<NewCylinder>(cylinder =>
                {
                    duplicate = new NewCylinder
                    {
                        Center = point3d,
                        Axis = cylinder.Axis,
                        Length = cylinder.Length,
                        Diameter = cylinder.Diameter,
                    };
                });
            selectedNewPrimitive.MatchClass<NewHalfSphere>(halfsphere =>
                {
                    duplicate = new NewHalfSphere
                    {
                        Center = point3d,
                        Radius = halfsphere.Radius,
                        Axis = halfsphere.Axis,
                        Length = halfsphere.Length,
                    };
                });
            
            Contract.Assert(duplicate != null);
            return duplicate;
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

                return true;
            }

            return false;
        }
    }
}
