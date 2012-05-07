using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class RectangleFeatureCurve : FeatureCurve
    {
        /// <summary>
        /// A term for the rectangle's width
        /// </summary>
        public Term Widgth { get; set; }

        /// <summary>
        /// A term for the rectangle's height
        /// </summary>
        public Term Height { get; set; }

        /// <summary>
        /// The optimized rectangle width
        /// </summary>
        public double WidthResult { get; set; }

        /// <summary>
        /// The optimized rectangle height
        /// </summary>
        public double HeightResult { get; set; }

        public override bool IsSnapped()
        {
            throw new NotImplementedException();
        }
    }
}
