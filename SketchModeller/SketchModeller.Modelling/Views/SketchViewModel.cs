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
using SketchModeller.Modelling.Events;
using System.Windows.Media.Media3D;
//namespace SketchModeller.Modelling.Views

namespace SketchModeller.Modelling.Views
{
    public class SketchViewModel : NotificationObject, IWeakEventListener
    {
        private readonly IEventAggregator eventAggregator;
        private readonly UiState uiState;
        private readonly SessionData sessionData;
        private readonly ISnapper snapper;
        protected ICurveAssigner curveAssigner;

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

            NewPrimitives = sessionData.NewPrimitives;

            SketchModellingViewModel = container.Resolve<SketchModellingViewModel>();
            SketchImageViewModel = container.Resolve<SketchImageViewModel>();

            eventAggregator.GetEvent<MarkFeatureEvent>().Subscribe(MarkFeatureHandler);
            eventAggregator.GetEvent<MarkSilhouetteEvent>().Subscribe(MarkSilhouetteHandler);
        }

        public ObservableCollection<NewPrimitive> NewPrimitives { get; set; }
        public SketchModellingViewModel SketchModellingViewModel { get; private set; }

        public SketchImageViewModel SketchImageViewModel { get; private set; }

        public void SnapPrimitive()
        {
            Work.Execute(eventAggregator, () => snapper.SnapAsync());
        }

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
                        newCylinder.Diameter.Value = 0.2;
                        newCylinder.Length.Value = 0.3;
                        sessionData.NewPrimitives.Add(newCylinder);
                        break;
                    case PrimitiveKinds.Cone:
                        var newCone = new NewCone();
                        newCone.Center.Value = pos3d.Value;
                        newCone.Axis.Value = sketchPlane.YAxis;
                        newCone.TopRadius.Value = 0.1;
                        newCone.BottomRadius.Value = 0.2;
                        newCone.Length.Value = 0.3;
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
                        newSGC.Length.Value = 0.3;
                        newSGC.Components = SgcComponents.CreateNonLinear(20, 0.075, 0.05, 0.1);
                        sessionData.NewPrimitives.Add(newSGC);
                        break;
                    case PrimitiveKinds.BGC:
                        var newBGC = new NewBendedGenCylinder();
                        newBGC.Center.Value = pos3d.Value;
                        newBGC.Axis.Value = sketchPlane.YAxis;
                        newBGC.Length.Value = 0.3;
                        newBGC.Components = BgcComponents.Create(20, 0.075, 0.15, newBGC.Center.Value, newBGC.Axis.Value, newBGC.Length.Value);
                        sessionData.NewPrimitives.Add(newBGC);
                        break;
                    case PrimitiveKinds.Cuboid:
                        var newCuboid = new NewCuboid();
                        Vector3D H = new Vector3D(0, 1, 0);
                        Vector3D W = new Vector3D(1, 0, 0);
                        Vector3D D = new Vector3D(0, 0, -1);
                        newCuboid.Center.Value = pos3d.Value;
                        newCuboid.H.Value = H;
                        newCuboid.W.Value = W;
                        newCuboid.D.Value = D;
                        newCuboid.Width.Value = 0.3;
                        newCuboid.Height.Value = 0.3;
                        newCuboid.Depth.Value = 0.3;
                        sessionData.NewPrimitives.Add(newCuboid);
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

        #region Composite event handlers

        private void MarkFeatureHandler(object dummy)
        {
            MarkAs(CurveCategories.Feature);
        }

        private void MarkSilhouetteHandler(object dummy)
        {
            MarkAs(CurveCategories.Silhouette);
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
