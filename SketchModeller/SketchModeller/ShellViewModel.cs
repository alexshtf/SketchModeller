using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;

namespace SketchModeller
{
    class ShellViewModel : NotificationObject
    {
        private bool isWorking;

        public ShellViewModel()
        {

        }

        public ShellViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<WorkingEvent>().Subscribe(OnWorkingEvent);
        }

        public bool IsWorking
        {
            get { return isWorking; }
            private set
            {
                isWorking = value;
                RaisePropertyChanged(() => IsWorking);
            }
        }

        private void OnWorkingEvent(bool isWorkingFlag)
        {
            IsWorking = isWorkingFlag;
        }
    }
}
