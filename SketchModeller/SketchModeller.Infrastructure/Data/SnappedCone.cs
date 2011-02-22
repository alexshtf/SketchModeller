﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    public class SnappedCone : SnappedPrimitive
    {
        #region Snapped cpne results

        public Point3D[] TopCircle { get; set; }
        public Point3D[] BottomCircle { get; set; }

        #endregion

        #region Optimization variables

        public TVec BottomCenter { get; set; }
        public TVec Axis { get; set; }
        public Variable Length { get; set; }
        public Variable BottomRadius { get; set; }
        public Variable TopRadius { get; set; }

        #endregion

        #region Optimization results

        public Point3D BottomCenterResult { get; set; }
        public Vector3D AxisResult { get; set; }
        public double LengthResult { get; set; }
        public double BottomRadiusResult { get; set; }
        public double TopRadiusResult { get; set; }

        #endregion
    }
}