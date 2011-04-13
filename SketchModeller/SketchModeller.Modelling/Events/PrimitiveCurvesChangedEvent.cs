using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Events
{
    /// <summary>
    /// Notifies that the data of primitive curves has changed.
    /// </summary>
    class PrimitiveCurvesChangedEvent : CompositePresentationEvent<NewPrimitive>
    {
    }
}
