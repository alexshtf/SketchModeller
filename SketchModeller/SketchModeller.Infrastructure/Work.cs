using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Reactive.Linq;

namespace SketchModeller.Infrastructure
{
    public static class Work
    {
        public static void Execute<T>(IEventAggregator eventAggregator, Func<IObservable<T>> workItem, Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            Contract.Requires(workItem != null);
            Contract.Requires(eventAggregator != null);
            
            // make sure subscription methods are not null
            if (onNext == null)
                onNext = _ => { };
            if (onCompleted == null)
                onCompleted = () => { };
            if (onError == null)
                onError = _ => { };

            // generate work id
            var workId = Guid.NewGuid();

            // wrap completion/error methods.
            Action wrappedOnCompleted = () =>
                {
                    try
                    {
                        onCompleted();
                    }
                    finally
                    {
                        eventAggregator.GetEvent<StopWorkingEvent>().Publish(workId);
                    }
                };
            Action<Exception> wrappedOnError = ex =>
                {
                    try
                    {
                        onError(ex);
                    }
                    finally
                    {
                        eventAggregator.GetEvent<StopWorkingEvent>().Publish(workId);
                    }
                };
            // perform the operation + subscribe on dispatcher
            workItem().ObserveOnDispatcher().Subscribe(onNext, wrappedOnError, wrappedOnCompleted);
            eventAggregator.GetEvent<StartWorkingEvent>().Publish(workId);
        }
    }
}
