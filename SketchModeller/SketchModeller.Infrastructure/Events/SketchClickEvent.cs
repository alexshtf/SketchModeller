using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// Information provided with the <see cref="SketchClickEvent"/>. Contains a 3D ray extended from the click point.
    /// </summary>
    public class SketchClickInfo
    {
        public SketchClickInfo(Point3D rayStart, Point3D rayEnd)
        {
            RayStart = rayStart;
            RayEnd = rayEnd;
        }

        /// <summary>
        /// Gets the ray's starting position
        /// </summary>
        public Point3D RayStart { get; private set; }

        /// <summary>
        /// Gets the ray's ending position.
        /// </summary>
        public Point3D RayEnd { get; private set; }
    }

    /// <summary>
    /// An event signifying a left-click on the sketch surface. The parameter contains information about the 3D
    /// ray extended from the click position. Useful for placing primitives on the sketch plane.
    /// </summary>
    public class SketchClickEvent : CompositePresentationEvent<SketchClickInfo>
    {
    }
}
