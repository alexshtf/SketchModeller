using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// The snapped (with variables) version of <see cref="CylinderComponent"/>
    /// </summary>
    [Serializable]
    public class SnappedCyliderComponent
    {
        private readonly Variable radius;
        private readonly double progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnappedCyliderComponent"/> class.
        /// </summary>
        /// <param name="radius">The radius variable</param>
        /// <param name="progress">The progress along the cylinder's length, between 0 and 1.</param>
        public SnappedCyliderComponent(Variable radius, double progress)
        {
            this.radius = radius;
            this.progress = progress;
        }

        /// <summary>
        /// Gets the radius variable
        /// </summary>
        /// <value>The radius variable</value>
        public Variable Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// Gets the progress along the cylinder's length
        /// </summary>
        public double Progress
        {
            get { return progress; }
        }
    }
}