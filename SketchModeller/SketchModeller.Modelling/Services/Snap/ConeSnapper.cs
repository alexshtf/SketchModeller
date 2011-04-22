using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using System.Diagnostics;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.Snap
{
    class ConeSnapper : CylindricalSnapper<NewCone, SnappedCone>
    {
        protected override void SpecificInit(NewCone newPrimitive, SnappedCone snapped)
        {
            snapped.TopRadius = new Variable();
            snapped.BottomRadius = new Variable();
            snapped.TopRadiusResult = newPrimitive.TopRadius;
            snapped.BottomRadiusResult = newPrimitive.BottomRadius;
        }

        protected override Term GetTopRadius(SnappedCone snapped)
        {
            return snapped.TopRadius;
        }

        protected override Term GetBottomRadius(SnappedCone snapped)
        {
            return snapped.BottomRadius;
        }

        protected override Term GetRadiusSoftConstraint(SnappedCone snapped, double expectedTop, double expectedBottom)
        {
            return 0.5 * (
                TermBuilder.Power(snapped.TopRadius - expectedTop, 2) + 
                TermBuilder.Power(snapped.BottomRadius - expectedBottom, 2));
        }
    }
}
