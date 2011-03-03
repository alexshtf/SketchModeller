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

namespace SketchModeller.Modelling.Views
{
    public class SketchViewModel : NotificationObject, IWeakEventListener
    {
        private readonly IEventAggregator eventAggregator;
        private readonly UiState uiState;
        private readonly SessionData sessionData;

        [InjectionConstructor]
        public SketchViewModel(IUnityContainer container, IEventAggregator eventAggregator, UiState uiState, SessionData sessionData)
        {
            this.uiState = uiState;
            this.sessionData = sessionData;
            this.eventAggregator = eventAggregator;

            uiState.AddListener(this, () => uiState.SketchPlane);
            sessionData.AddListener(this, () => sessionData.SketchName);

            SketchModellingViewModel = container.Resolve<SketchModellingViewModel>();
            SketchImageViewModel = container.Resolve<SketchImageViewModel>();
        }

        public SketchModellingViewModel SketchModellingViewModel { get; private set; }

        public SketchImageViewModel SketchImageViewModel { get; private set; }

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

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;
            if (eventArgs.Match(() => uiState.SketchPlane))
                SketchPlane = uiState.SketchPlane;

            return true;
        }

        public void AddNewPrimitive(PrimitiveKinds primitiveKind, LineRange lineRange)
        {
            var pos3d = uiState.SketchPlane.PointFromRay(lineRange);
            if (pos3d != null)
            {
                switch (primitiveKind)
                {
                    case PrimitiveKinds.Cylinder:
                        sessionData.NewPrimitives.Add(new NewCylinder
                            {
                                Center = pos3d.Value,
                                Axis = sketchPlane.YAxis,
                                Diameter = 0.4,
                                Length = 0.6,
                            });
                        break;
                    case PrimitiveKinds.Cone:
                        sessionData.NewPrimitives.Add(new NewCone
                            {
                                Center = pos3d.Value,
                                Axis = sketchPlane.YAxis,
                                BottomRadius = 0.2,
                                TopRadius = 0.2,
                                Length = 0.6,
                            });
                        break;
                    default:
                        Trace.Fail("Invalid primitive kind");
                        break;
                }
            }
        }

        public void DeleteNewPrimitives()
        {
            var selectedPrimitives = sessionData.SelectedNewPrimitives.ToArray();
            foreach (var item in selectedPrimitives)
                sessionData.NewPrimitives.Remove(item);
        }
    }
}
