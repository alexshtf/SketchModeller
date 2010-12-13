using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Diagnostics;
using System.Windows;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Modelling;
using System.ComponentModel;
using Utils;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller
{
    class ShellViewModel : NotificationObject, IWeakEventListener
    {
        private HashSet<Guid> workingIds;
        private UiState uiState;
        private IEventAggregator eventAggregator;

        public ShellViewModel()
        {
            workingIds = new HashSet<Guid>();
        }

        public ShellViewModel(IEventAggregator eventAggregator, UiState uiState)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.uiState = uiState;
            uiState.AddListener(this, () => uiState.SketchPlane);

            eventAggregator.GetEvent<StartWorkingEvent>().Subscribe(OnStartWorking);
            eventAggregator.GetEvent<StopWorkingEvent>().Subscribe(OnStopWorking);
        }

        public bool IsWorking
        {
            get { return workingIds.Count > 0; }
        }

        public void OnSketchClick(Point3D p1, Point3D p2)
        {
            var payload = new SketchClickInfo(p1, p2);
            eventAggregator.GetEvent<SketchClickEvent>().Publish(payload);
        }

        private void OnStartWorking(Guid workId)
        {
            Trace.Assert(!workingIds.Contains(workId), "Cannot notify startup of an already running work");
            
            workingIds.Add(workId);
            RaisePropertyChanged(() => IsWorking);
        }

        private void OnStopWorking(Guid workId)
        {
            Trace.Assert(workingIds.Contains(workId), "Cannot notify stopping of a non-running work");
            
            workingIds.Remove(workId);
            RaisePropertyChanged(() => IsWorking);
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

        #region Event handling

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;
            if (eventArgs.Match(() => uiState.SketchPlane))
                OnSketchPlaneChanged();

            return true;
        }

        private void OnSketchPlaneChanged()
        {
            SketchPlane = uiState.SketchPlane;
        }

        #endregion

    }
}
