using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// En event signifying that an asynchronous work has started. Fired by the different components and used by the shell
    /// to disable the UI. The parameter is an ID of the work item.
    /// </summary>
    public class StartWorkingEvent : CompositePresentationEvent<Guid>
    {
    }
}
