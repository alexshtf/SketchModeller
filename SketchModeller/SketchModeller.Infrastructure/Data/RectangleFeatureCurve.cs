﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Windows.Media.Media3D;

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
        /// A term vector for the rectangle's width vector
        /// </summary>
        public TVec WidthVector { get; set; }

        /// <summary>
        /// The optimized rectangle width
        /// </summary>
        public double WidthResult { get; set; }

        /// <summary>
        /// The optimized rectangle height
        /// </summary>
        public double HeightResult { get; set; }

        /// <summary>
        /// The optimized rectangle width vector
        /// </summary>
        public Vector3D WidthVectorResult { get; set; }

        /// <summary>
        /// The sketch curves this feature curve is snapped to
        /// </summary>
        public PointsSequence[] SnappedTo { get; set; }

        public override bool IsSnapped()
        {
            return SnappedTo != null && SnappedTo.Length > 0;
        }
    }
}
