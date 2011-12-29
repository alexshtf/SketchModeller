using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Petzold.Media3D;

namespace SketchModeller.Modelling
{
    /// <summary>
    /// The interface between the application and the editor of a specific primitive.
    /// </summary>
    public interface IEditor
    {
        /// <summary>
        /// Sends a mouse drag message to the primitive editor.
        /// </summary>
        /// <param name="currPos">The mouse 2D position</param>
        /// <param name="currRay">The 3D ray</param>
        void Drag(Point currPos, LineRange currRay);
    }
}
