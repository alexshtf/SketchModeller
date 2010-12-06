using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Diagnostics;

namespace SketchModeller
{
    class ShellViewModel : NotificationObject
    {
        private HashSet<Guid> workingIds;

        public ShellViewModel()
        {
            workingIds = new HashSet<Guid>();
        }

        public ShellViewModel(IEventAggregator eventAggregator)
            : this()
        {
            eventAggregator.GetEvent<StartWorkingEvent>().Subscribe(OnStartWorking);
            eventAggregator.GetEvent<StopWorkingEvent>().Subscribe(OnStopWorking);
        }

        public bool IsWorking
        {
            get { return workingIds.Count > 0; }
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
    }
}
