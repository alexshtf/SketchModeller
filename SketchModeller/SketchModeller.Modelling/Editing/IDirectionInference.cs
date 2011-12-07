using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Modelling.Editing
{
    /// <summary>
    /// An interface to a drag direction inference engine. The user can provide
    /// points where the mouse cursor has passed and afterwards ask the engine
    /// to infer direction.
    /// </summary>
    interface IDirectionInference
    {
        /// <summary>
        /// Resets the engine to the state before any point has been provided.
        /// </summary>
        void Reset();

        /// <summary>
        /// Provides a point where the user dragged his mouse.
        /// </summary>
        /// <param name="pnt">The provided point.</param>
        void ProvidePoint(Point pnt);

        /// <summary>
        /// Attempt to infer the direction from the last provided points.
        /// </summary>
        /// <returns>The inferred drag direction or <c>null</c> if no 
        /// conclusive direction could be inferred.</returns>
        Vector? InferDirection();
    }
}
