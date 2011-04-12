using System;
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

        public Point[] Points
        {
            get { return points; }
            set { points = value; }
        }

        public PointsSequence AssignedTo
        {
            get { return assignedTo; }
            set { assignedTo = value; }
        }
    }
}
