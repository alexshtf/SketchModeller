using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Utilities;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Computations
{
    static class EllipseHelper
    {
        /// <summary>
        /// Approximates the perimeter of an ellipse given the lengths of its two major/minor
        /// axes.
        /// </summary>
        /// <param name="a">The length of the first axis</param>
        /// <param name="b">The length of the second axis</param>
        /// <returns>An approximation of the perimeter of an ellipse.</returns>
        /// <remarks>The formula was taken from http://www.mathsisfun.com/geometry/ellipse-perimeter.html. </remarks>
        public static double ApproxPerimeter(double a, double b)
        {
            Contract.Requires(a >= 0);
            Contract.Requires(b >= 0);
            Contract.Ensures(Contract.Result<double>() >= a);
            Contract.Ensures(Contract.Result<double>() >= b);

            var h = Math.Pow(a - b, 2) / Math.Pow(a + b, 2);
            var p = Math.PI * (a + b) * (
                1 + 
                h / 4 + 
                h * h / 64 + 
                h * h * h / 256 + 
                25 * h * h * h * h / 16384);
            return p;
        }

        /// <summary>
        /// Computes 3D circle orientation given its projection as a 2D ellipse.
        /// </summary>
        /// <param name="ellipseParams">The parameters of the 2D ellipse</param>
        /// <returns>A tuple containing two basis vectors for the circle's plane. The second
        /// vector is correct up to the sign of its Z component.</returns>
        public static Tuple<Vector3D, Vector3D> CircleOrientation(EllipseParams ellipseParams)
        {
            Contract.Ensures(Contract.Result<Tuple<Vector3D, Vector3D>>() != null);

            var rotation = Matrix.Identity;
            rotation.Rotate(ellipseParams.Degrees);

            var a = rotation.Transform(new Vector(ellipseParams.XRadius, 0));
            var b = rotation.Transform(new Vector(0, ellipseParams.YRadius));

            // we swap a and b in case Y radius is larger than X radius
            // becuase we want a to be the major axis.
            if (ellipseParams.YRadius > ellipseParams.XRadius)
            {
                var temp = a;
                a = b;
                b = temp;
            }

            var result1 = new Vector3D(a.X, a.Y, 0);
            var result2 = new Vector3D(b.X, b.Y, Math.Sqrt(a.LengthSquared - b.LengthSquared));
            return Tuple.Create(result1, result2);
        }
    }
}
