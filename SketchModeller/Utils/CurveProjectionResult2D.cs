using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Utils
{
    /// <summary>
    /// Represents the result of a projection of a point on a curve in 3D
    /// </summary>
    public class CurveProjectionResult2D
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurveProjectionResult2D"/> class.
        /// </summary>
        /// <param name="position">The projected position.</param>
        /// <param name="segmentIndex">The index of the segment on the curve containing the projected point.</param>
        /// <param name="distance">The distance between the source point and <paramref name="position"/>.</param>
        public CurveProjectionResult2D(Point position, int segmentIndex, double distance)
        {
            Position = position;
            SegmentIndex = SegmentIndex;
            Distance = distance;
        }

        /// <summary>
        /// The projected position.
        /// </summary>
        public Point Position { get; private set; }

        /// <summary>
        /// The index of the segment on the curve containing the projected point
        /// </summary>
        public int SegmentIndex { get; private set; }

        /// <summary>
        /// The distance between the source point and <see cref="Position"/>.
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var concrete = obj as CurveProjectionResult2D;
            if (concrete == null)
                return false;
            else
            {
                var result =
                    concrete.Distance == Distance &&
                    concrete.Position == Position &&
                    concrete.SegmentIndex == SegmentIndex;
                return result;
            }
        }

        public override int GetHashCode()
        {
            return
                Position.GetHashCode() ^
                SegmentIndex.GetHashCode() ^
                Distance.GetHashCode();
        }
    }
}
