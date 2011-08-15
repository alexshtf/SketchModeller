using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Unity;
using System.Windows;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.ComponentModel;
using Utils;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure;
using Petzold.Media3D;
using SketchModeller.Utilities;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SketchModeller.Infrastructure.Services;
using System.Collections.ObjectModel;
using SketchModeller.Modelling.Computations;

namespace SketchModeller.Modelling.Views
{
    public class SketchViewModel : NotificationObject, IWeakEventListener
    {
        private readonly IEventAggregator eventAggregator;
        private readonly UiState uiState;
        private readonly SessionData sessionData;
        private readonly ISnapper snapper;

        [InjectionConstructor]
        public SketchViewModel(
            IUnityContainer container, 
            IEventAggregator eventAggregator, 
            UiState uiState, 
            SessionData sessionData,
            ISnapper snapper)
        {
            this.uiState = uiState;
            this.sessionData = sessionData;
            this.eventAggregator = eventAggregator;
            this.snapper = snapper;

            uiState.AddListener(this, () => uiState.SketchPlane);
            sessionData.AddListener(this, () => sessionData.SketchName);

            NewPrimitives = sessionData.NewPrimitives.AsReadOnlyObservable();

            SketchModellingViewModel = container.Resolve<SketchModellingViewModel>();
            SketchImageViewModel = container.Resolve<SketchImageViewModel>();

            DeletePrimitive = new DelegateCommand(DeletePrimitiveExecute, DeletePrimitiveCanExecute);
            SnapPrimitive = new DelegateCommand(SnapPrimitiveExecute, SnapPrimitiveCanExecute);
            MarkFeature = new DelegateCommand(MarkFeatureExecute, MarkFeatureCanExecute);
            MarkSilhouette = new DelegateCommand(MarkSilhouetteExecute, MarkSilhouetteCanExecute);
        }

        public ReadOnlyObservableCollection<NewPrimitive> NewPrimitives { get; private set; }
        public SketchModellingViewModel SketchModellingViewModel { get; private set; }

        public SketchImageViewModel SketchImageViewModel { get; private set; }

        public ICommand DeletePrimitive { get; private set; }
        public ICommand SnapPrimitive { get; private set; }

        public ICommand MarkFeature { get; private set; }
        public ICommand MarkSilhouette { get; private set; }

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

        #region IsPreviewing property

        private bool isPreviewing;

        public bool IsPreviewing
        {
            get { return isPreviewing; }
            set
            {
                isPreviewing = value;
                RaisePropertyChanged(() => IsPreviewing);
            }
        }

        #endregion

        #region Primitive add/delete

        public void AddNewPrimitive(PrimitiveKinds primitiveKind, LineRange lineRange)
        {
            var pos3d = uiState.SketchPlane.PointFromRay(lineRange);
            if (pos3d != null)
            {
                switch (primitiveKind)
                {
                    case PrimitiveKinds.Cylinder:
                        var newCylinder = new NewCylinder();
                        newCylinder.Center.Value = pos3d.Value;
                        newCylinder.Axis.Value = sketchPlane.YAxis;
                        newCylinder.Diameter.Value = 0.4;
                        newCylinder.Length.Value = 0.6;
                        sessionData.NewPrimitives.Add(newCylinder);
                        break;
                    case PrimitiveKinds.Cone:
                        var newCone = new NewCone();
                        newCone.Center.Value = pos3d.Value;
                        newCone.Axis.Value = sketchPlane.YAxis;
                        newCone.BottomRadius.Value = 0.2;
                        newCone.TopRadius.Value = 0.2;
                        newCone.Length.Value = 0.6;
                        sessionData.NewPrimitives.Add(newCone);
                        break;
                    case PrimitiveKinds.Sphere:
                        var newSphere = new NewSphere();
                        newSphere.Center.Value = pos3d.Value;
                        newSphere.Radius.Value = 0.2;
                        sessionData.NewPrimitives.Add(newSphere);
                        break;
                    case PrimitiveKinds.SGC:
                        var newSGC = new NewStraightGenCylinder();
                        newSGC.Center.Value = pos3d.Value;
                        newSGC.Axis.Value = sketchPlane.YAxis;
                        newSGC.Length.Value = 0.6;
                        newSGC.Components = SgcComponents.Create(20, 0.2, 0.6);
                        sessionData.NewPrimitives.Add(newSGC);
                        break;
                    default:
                        Trace.Fail("Invalid primitive kind");
                        break;
                }

                if (sessionData.NewPrimitives.Any())
                    sessionData.NewPrimitives.Last().UpdateCurvesGeometry();
            }
        }

        public void DeleteNewPrimitives()
        {
            var selectedPrimitives = sessionData.SelectedNewPrimitives.ToArray();
            foreach (var item in selectedPrimitives)
                sessionData.NewPrimitives.Remove(item);
        }
        
        #endregion

        #region Primitive selection

        public void SelectPrimitive(SelectablePrimitive toSelect)
        {
            UnselectPrimitives();
            toSelect.IsSelected = true;
        }

        public void UnselectPrimitives()
        {
            var toUnSelect = sessionData.SelectedPrimitives.ToArray();
            foreach (var item in toUnSelect)
                item.IsSelected = false;
        }

        #endregion

        #region Commands execute

        private void DeletePrimitiveExecute()
        {
            DeleteNewPrimitives();
        }

        private bool DeletePrimitiveCanExecute()
        {
            return true;
        }

        private void SnapPrimitiveExecute()
        {
            snapper.Snap();
        }

        private bool SnapPrimitiveCanExecute()
        {
            return true;
        }

        private void MarkFeatureExecute()
        {
            MarkAs(CurveCategories.Feature);
        }

        private bool MarkFeatureCanExecute()
        {
            return true;
        }

        private void MarkSilhouetteExecute()
        {
            MarkAs(CurveCategories.Silhouette);
        }

        private bool MarkSilhouetteCanExecute()
        {
            return true;
        }

        #endregion

        private void MarkAs(CurveCategories newCategory)
        {
            foreach (var curve in sessionData.SelectedSketchObjects)
                curve.CurveCategory = newCategory;
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

    }
}
