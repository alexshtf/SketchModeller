using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Modelling.Services.Duplicator
{
    class CylinderCylinderDuplicator: BaseDuplicator<SnappedCylinder, NewCylinder>
    {
        public override bool IsNatural
        {
            get { return true; }
        }

        protected override NewCylinder DuplicateCore(SnappedCylinder snapped)
        {
            var newCylinder = new NewCylinder
            {
                Axis = snapped.AxisResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
                Length = snapped.LengthResult,
                Diameter = 2 * snapped.RadiusResult,
            };
            return newCylinder;
        }
    }

    class CylinderConeDuplicator : BaseDuplicator<SnappedCylinder, NewCone>
    {
        public override bool IsNatural
        {
            get { return false; }
        }

        protected override NewCone DuplicateCore(SnappedCylinder snapped)
        {
            var result = new NewCone
            {
                Axis = snapped.AxisResult,
                Length = snapped.LengthResult,
                Center = MathUtils3D.Lerp(snapped.BottomCenterResult, snapped.TopCenterResult, 0.5),
                TopRadius = snapped.RadiusResult,
                BottomRadius = snapped.RadiusResult,
            };
            return result;
        }
    }
}
