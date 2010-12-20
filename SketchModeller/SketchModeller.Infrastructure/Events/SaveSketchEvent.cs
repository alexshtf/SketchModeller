using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// An event signaling that the currently loaded sketch should be saved. Has no parameters.
    /// </summary>
    public class SaveSketchEvent : CompositePresentationEvent<object>
    {
    }
}
