using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// En event signifying that an asynchronous work has ended. Fired by the different components and used by the shell
    /// to enable the UI. The parameter is an ID of the work item.
    /// </summary>
    public class StopWorkingEvent : CompositePresentationEvent<Guid>
    {
    }
}
