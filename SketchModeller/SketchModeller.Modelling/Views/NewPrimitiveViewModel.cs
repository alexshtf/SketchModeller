using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure;
using System.Collections.ObjectModel;
using SketchModeller.Infrastructure.Data;

using UiState = SketchModeller.Infrastructure.Shared.UiState;
using SketchPlane = SketchModeller.Infrastructure.Data.SketchPlane;
using System.Windows;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Utilities;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Modelling.Events;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Services;
using System.ComponentModel;

namespace SketchModeller.Modelling.Views
{
    abstract public class NewPrimitiveViewModel : NotificationObject, IWeakEventListener
    {
        private NewPrimitive model;
        protected UiState uiState;
        protected IEventAggregator eventAggregator;
        protected ICurveAssigner curveAssigner;

        public NewPrimitiveViewModel(UiState uiState, ICurveAssigner curveAssigner, IEventAggregator eventAggregator)
        {
            ContextMenu = new ObservableCollection<MenuCommandData>();
            this.uiState = uiState;
            this.curveAssigner = curveAssigner;
            this.eventAggregator = eventAggregator;
        }

        public ObservableCollection<MenuCommandData> ContextMenu { get; private set; }

        public NewPrimitive Model 
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    model.RemoveListener(this, () => model.IsSelected);
                    if (model.IsSelected)
                        model.ClearColorCodingFromSketch();
                }

                model = value;

                if (model != null)
                {
                    model.AddListener(this, () => model.IsSelected);
                    if (model.IsSelected)
                        model.SetColorCodingToSketch();
                }
            }
        }
        public SketchPlane SketchPlane { get { return uiState.SketchPlane; } }

        public abstract void UpdateFromModel();

        public void NotifyDragged()
        {
            Model.UpdateCurvesGeometry();
            ComputeCurvesAssignment();
            eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Publish(Model);
        }

        private void ComputeCurvesAssignment()
        {
            if (Model.IsSelected)
                Model.ClearColorCodingFromSketch();

            curveAssigner.ComputeAssignments(Model);
            
            if (Model.IsSelected)
                Model.SetColorCodingToSketch();
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            if (Model.IsSelected)
                Model.SetColorCodingToSketch();
            else
                Model.ClearColorCodingFromSketch();

            return true;
        }
    }
}
