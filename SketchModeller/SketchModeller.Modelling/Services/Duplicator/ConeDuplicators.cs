using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Modelling.Services.Duplicator
{
    class ConeConeDuplicator : BaseDuplicator<SnappedCone, NewCone>
    {
        public override bool IsNatural
        {
            get { return true; }
        }

        protected override NewCone DuplicateCore(SnappedCone snapped)
        {
            var newCone = new NewCone
            {
                Axis = snapped.AxisResult,
                TopRadius = snapped.TopRadiusResult,
                BottomRadius = snapped.BottomRadiusResult,
                Length = snapped.LengthResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
            };
            return newCone;
        }
    }

    class ConeCylinderDuplicator : BaseDuplicator<SnappedCone, NewCylinder>
    {
        protected override NewCylinder DuplicateCore(SnappedCone snapped)
        {
            var newCylinder = new NewCylinder
            {
                Axis = snapped.AxisResult,
                Diameter = snapped.TopRadiusResult + snapped.BottomRadiusResult,
                Length = snapped.LengthResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
            };
            return newCylinder;
        }

        public override bool IsNatural
        {
            get { return false; }
        }
    }

}
