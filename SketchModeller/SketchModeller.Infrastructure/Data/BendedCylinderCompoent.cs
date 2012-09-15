using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A component of a cylindrical shape which is made of many circular components and the spine data points
    /// </summary>
    [Serializable]
    public class BendedCylinderComponent : CylinderComponent    
    {
        private double s;
        private double t;
        /// <summary>
        /// Initializes a new instance of the <see cref="BendedCylinderComponent"/> class.
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="progress">The progress, on the cylinder's spine, of this circle (between 0 and 1)</param>
        public BendedCylinderComponent(double radius, double progress, double s, double t) : base(radius, progress)
        {
            Contract.Ensures(S == s);
            Contract.Ensures(T == t);
            this.s = s;
            this.t = t;
        }

        /// <summary>
        /// Gets the Spine's 3D point
        /// </summary>
        public double S { get { return s; } set { s = value; } }
        public double T { get { return t; } set { t = value; } }
    }
}
