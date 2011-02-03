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
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure;

namespace SketchModeller
{
    class ShellViewModel : NotificationObject, IWeakEventListener
    {
        private const string TITLE_FORMAT = "Editing {0}";

        private HashSet<Guid> workingIds;
        private SessionData sessionData;
        private IEventAggregator eventAggregator;

        public ShellViewModel()
        {
            workingIds = new HashSet<Guid>();
        }

        [InjectionConstructor]
        public ShellViewModel(IEventAggregator eventAggregator, SessionData sessionData)
            : this()
        {
            this.eventAggregator = eventAggregator;
            this.sessionData = sessionData;

            eventAggregator.GetEvent<StartWorkingEvent>().Subscribe(OnStartWorking);
            eventAggregator.GetEvent<StopWorkingEvent>().Subscribe(OnStopWorking);
            sessionData.AddListener(this, () => sessionData.SketchName);
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

        #region Event handling

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;

            if (eventArgs.Match(() => sessionData.SketchName))
                Title = string.Format(TITLE_FORMAT, sessionData.SketchName);

            return true;
        }

        #endregion
    }
}
