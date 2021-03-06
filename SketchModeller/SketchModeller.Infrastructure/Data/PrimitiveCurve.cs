﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class PrimitiveCurve
    {
        private Point[] points;
        private PointsSequence assignedTo;
        private Point closestPoint;
        private bool isUserAssignment;
        public bool isDeselected = false;
        public Point[] Points
        {
            get { return points; }
            set { points = value; }
        }

        public Point ClosestPoint
        {
            get { return closestPoint; }
            set { closestPoint = value; }
        }

        public PointsSequence AssignedTo
        {
            get { return assignedTo; }
            set { assignedTo = value; }
        }

        public bool IsUserAssignment
        {
            get { return isUserAssignment; }
            set { isUserAssignment = true; }
        }
    }
}
