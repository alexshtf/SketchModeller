using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SimpleCurveEdit
{
    interface ICurve
    {
        Point3DCollection Points { get; set; }
    }
}
