using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A component of a cylindrical shape which is made of many circular components and the spine data points
    /// </summary>
    [Serializable]
    public class BendedCylinderComponent : CylinderComponent    
    {
        private Point3D pnt3D;
        private readonly Point pnt2D;
        /// <summary>
        /// Initializes a new instance of the <see cref="BendedCylinderComponent"/> class.
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="progress">The progress, on the cylinder's spine, of this circle (between 0 and 1)</param>
        public BendedCylinderComponent(double radius, double progress, Point3D pnt3D, Point pnt2D) : base(radius, progress)
        {
            Contract.Ensures(Pnt3D == pnt3D);
            Contract.Ensures(Pnt2D == pnt2D);
            this.pnt2D = pnt2D;
            this.pnt3D = pnt3D;
        }

        /// <summary>
        /// Gets the Spine's 3D point
        /// </summary>
        public Point3D Pnt3D { get { return pnt3D; } set { pnt3D = value; } }

        /// <summary>
        /// Gets the Spine's 3D point
        /// </summary>
        public Point Pnt2D { get { return pnt2D; } }
    }
}
