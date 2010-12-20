using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// A presentation event fired to signal that a sketch should be loaded. The string contains the sketch name.
    /// </summary>
    public class LoadSketchEvent : CompositePresentationEvent<string>
    {
    }
}
