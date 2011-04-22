using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using Utils;
using System.Diagnostics;
using SketchModeller.Utilities;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.Snap
{
    class CylinderSnapper : CylindricalSnapper<NewCylinder, SnappedCylinder>
    {
        protected override void SpecificInit(NewCylinder newPrimitive, SnappedCylinder snapped)
        {
            snapped.Radius = new Variable();
            snapped.RadiusResult = newPrimitive.Radius;
        }

        protected override Term GetTopRadius(SnappedCylinder snapped)
        {
            return snapped.Radius;
        }

        protected override Term GetBottomRadius(SnappedCylinder snapped)
        {
            return snapped.Radius;
        }

        protected override Term GetRadiusSoftConstraint(SnappedCylinder snapped, double expectedTop, double expectedBottom)
        {
            var avg = 0.5 * (expectedTop + expectedBottom);
            return TermBuilder.Power(snapped.Radius - avg, 2);
        }
    }
}
