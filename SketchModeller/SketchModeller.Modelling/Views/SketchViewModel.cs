﻿using System;
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

namespace SketchModeller.Modelling.Views
{
    public enum MouseInterationModes
    {
        CurveSelection,
        PrimitiveManipulation,
        FeatureCurveSelection,
    }

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

            SketchModellingViewModel = container.Resolve<SketchModellingViewModel>();
            SketchImageViewModel = container.Resolve<SketchImageViewModel>();

            DeletePrimitive = new DelegateCommand(DeletePrimitiveExecute, DeletePrimitiveCanExecute);
            SnapPrimitive = new DelegateCommand(SnapPrimitiveExecute, SnapPrimitiveCanExecute);
            MarkFeature = new DelegateCommand(MarkFeatureExecute, MarkFeatureCanExecute);
            MarkSilhouette = new DelegateCommand(MarkSilhouetteExecute, MarkSilhouetteCanExecute);
            CycleMouseInteractionMode = new DelegateCommand(CycleMouseInteractionModeExecute);
        }

        public SketchModellingViewModel SketchModellingViewModel { get; private set; }

        public SketchImageViewModel SketchImageViewModel { get; private set; }

        public ICommand DeletePrimitive { get; private set; }
        public ICommand SnapPrimitive { get; private set; }

        public ICommand MarkFeature { get; private set; }
        public ICommand MarkSilhouette { get; private set; }

        public ICommand CycleMouseInteractionMode { get; private set; }

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

        #region MouseInteractionMode property

        private MouseInterationModes mouseInteractionMode;

        public MouseInterationModes MouseInteractionMode
        {
            get { return mouseInteractionMode; }
            set
            {
                mouseInteractionMode = value;
                RaisePropertyChanged(() => MouseInteractionMode);
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

        private void CycleMouseInteractionModeExecute()
        {
            int modesCount = Enum.GetValues(typeof(MouseInterationModes)).Length;
            int modeNumber = (int)MouseInteractionMode;
            modeNumber = (modeNumber + 1) % modesCount;
            MouseInteractionMode = (MouseInterationModes)modeNumber;
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
