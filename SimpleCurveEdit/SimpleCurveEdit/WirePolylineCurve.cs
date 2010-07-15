using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Petzold.Media3D;
using System.Windows.Media.Media3D;

namespace SimpleCurveEdit
{
    class WirePolylineCurve : WirePolyline, ICurve
    {
        Point3DCollection ICurve.Points
        {
            get { return Points; }
            set { Points = value; }
        }
    }
}
