using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Generates points in a plane around a center using polar (angle, radius) representation.
    /// </summary>
    public class PolarPointsGenerator
    {
        private readonly Point3D center;
        private readonly Vector3D xBase;
        private readonly Vector3D yBase;

        /// <summary>
        /// Constructs a new instance of the <see cref="PolarPointsGenerator"/> class.
        /// </summary>
        /// <param name="center">The central point around which this class will generate other points.</param>
        /// <param name="xBase">Basis vector that serves as the X axis</param>
        /// <param name="yBase">Basis vector that serves as the Y axis</param>
        /// <remarks><paramref name="xBase"/> and <paramref name="yBase"/> vectors must be orthogonal</remarks>
        public PolarPointsGenerator(Point3D center, Vector3D xBase, Vector3D yBase)
        {
            Contract.Requires(xBase.LengthSquared > 0);
            Contract.Requires(yBase.LengthSquared > 0);
            Contract.Requires(MathUtils3D.AreOrthogonal(xBase, yBase));

            this.center = center;
            xBase.Normalize();
            yBase.Normalize();

            this.xBase = xBase;
            this.yBase = yBase;
        }

        /// <summary>
        /// Generates multiple points given their angles and distances.
        /// </summary>
        /// <param name="anglesRadians">The array of angles. See <see cref="GetPoint"/>.</param>
        /// <param name="distances">The array of distances. See <see cref="GetPoint"/>.</param>
        /// <returns>An array of generated points.</returns>
        public Point3D[] GetPoints(double[] anglesRadians, double[] distances)
        {
            Contract.Requires(anglesRadians != null);
            Contract.Requires(distances != null);
            Contract.Requires(anglesRadians.Length == distances.Length);
            Contract.Requires(Contract.ForAll(distances, distance => distance >= 0));

            Contract.Ensures(Contract.Result<Point3D[]>() != null);
            Contract.Ensures(Contract.Result<Point3D[]>().Length == distances.Length);

            var length = anglesRadians.Length;
            var result = new Point3D[length];
            foreach(var i in Enumerable.Range(0, length))
                result[i] = GetPoint(anglesRadians[i], distances[i]);

            return result;
        }

        /// <summary>
        /// Gets a single point given an angle and the distance from the center.
        /// </summary>
        /// <param name="angleRadians">Angle of the point from the X axis towards the Y axis.</param>
        /// <param name="distance">Distance from the center.</param>
        /// <returns>The resulting point.</returns>
        public Point3D GetPoint(double angleRadians, double distance)
        {
            Contract.Requires(distance >= 0);

            var direction = Math.Cos(angleRadians) * xBase + Math.Sin(angleRadians) * yBase;
            direction.Normalize();

            return center + distance * direction;
        }
    }
}
