using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Events
{
    public class SketchClickInfo
    {
        public SketchClickInfo(Point3D rayStart, Point3D rayEnd)
        {
            RayStart = rayStart;
            RayEnd = rayEnd;
        }

        public Point3D RayStart { get; private set; }
        public Point3D RayEnd { get; private set; }
    }

    public class SketchClickEvent : CompositePresentationEvent<SketchClickInfo>
    {
    }
}
