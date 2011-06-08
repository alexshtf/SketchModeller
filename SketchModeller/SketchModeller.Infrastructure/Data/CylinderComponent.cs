using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A component of a cylindrical shape which is made of many circular components.
    /// </summary>
    [Serializable]
    public class CylinderComponent
    {
        private readonly double radius;
        private readonly double progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="CylinderComponent"/> class.
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="progress">The progress, on the cylinder's spine, of this circle (between 0 and 1)</param>
        public CylinderComponent(double radius, double progress)
        {
            Contract.Requires(radius > 0);
            Contract.Requires(progress >= 0 && progress <= 1);
            Contract.Ensures(Radius == radius);
            Contract.Ensures(Progress == progress);

            this.radius = radius;
            this.progress = progress;
        }

        /// <summary>
        /// Gets the circle's radius
        /// </summary>
        public double Radius { get { return radius; } }

        /// <summary>
        /// Gets the circle's progress along the cylinder's spine.
        /// </summary>
        public double Progress { get { return progress; } }
    }
}
