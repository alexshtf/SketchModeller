using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SketchModeller.Infrastructure
{
    public static class Constants
    {
        /// <summary>
        /// The resulution of the distance-transform images. For more info see <see cref="Shared.SessionData.DistanceTransforms"/> property.
        /// </summary>
        public const int DISTANCE_TRANSFORM_RESOLUTION = 512;

        public static readonly Brush[] PRIMITIVE_CURVES_COLOR_CODING = 
        {
            Brushes.Green,
            Brushes.Goldenrod,
            Brushes.Olive,
            Brushes.Orchid,
            Brushes.SlateBlue,
            Brushes.SlateGray,
            Brushes.SteelBlue,
            Brushes.Peru,
            Brushes.Lime,
            Brushes.Firebrick,
        };
    }
}
