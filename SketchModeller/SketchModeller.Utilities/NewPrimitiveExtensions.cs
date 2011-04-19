using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Infrastructure.Data
{
    public static class NewPrimitiveExtensions
    {
        public static void SetColorCodingToSketch(this NewPrimitive primitive)
        {
            foreach (var pair in primitive.AllCurves.ZipIndex())
            {
                var curve = pair.Value;
                var index = pair.Index;
                
                if (curve.AssignedTo != null)
                    curve.AssignedTo.ColorCodingIndex = index;
            }
        }

        public static void ClearColorCodingFromSketch(this NewPrimitive primitive)
        {
            var assignedQuery =
                from curves in primitive.AllCurves
                where curves.AssignedTo != null
                select curves.AssignedTo;

            foreach (var sketchCurve in assignedQuery)
                sketchCurve.ColorCodingIndex = PointsSequence.INVALID_COLOR_CODING;
        }
    }
}
