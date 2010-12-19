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
        private const string TITLE_FORMAT = "Editing {0}";

        private HashSet<Guid> workingIds;
        private UiState uiState;
        private SessionData sessionData;
        private IEventAggregator eventAggregator;

        public ShellViewModel()
        {
            workingIds = new HashSet<Guid>();
        }

        public ShellViewModel(IEventAggregator eventAggregator, UiState uiState, SessionData sessionData)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.uiState = uiState;
            this.sessionData = sessionData;

            uiState.AddListener(this, () => uiState.SketchPlane);
            sessionData.AddListener(this, () => sessionData.SketchName);

            eventAggregator.GetEvent<StartWorkingEvent>().Subscribe(OnStartWorking);
            eventAggregator.GetEvent<StopWorkingEvent>().Subscribe(OnStopWorking);
        }

        public bool IsWorking
        {
            get { return workingIds.Count > 0; }
        }

        #region Title property

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        #endregion

        public void OnSketchClick(System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2)
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

            if (eventArgs.Match(() => sessionData.SketchName))
                Title = string.Format(TITLE_FORMAT, sessionData.SketchName);

            return true;
        }

        private void OnSketchPlaneChanged()
        {
            SketchPlane = uiState.SketchPlane;
        }

        #endregion

    }
}
