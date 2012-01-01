using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.Snap
{
    class PrimitivesReader : IPrimitivesReader
    {
        public void Read(double[] values, IEnumerable<SnappedPrimitive> primitives)
        {
            var resultReader = new VectorsReader(values);
            foreach (var snappedCylinder in primitives.OfType<SnappedCylinder>())
                resultReader.Read(snappedCylinder);

            foreach (var snappedCone in primitives.OfType<SnappedCone>())
                resultReader.Read(snappedCone);

            foreach (var snappedSphere in primitives.OfType<SnappedSphere>())
                resultReader.Read(snappedSphere);

            foreach (var snappedSgc in primitives.OfType<SnappedStraightGenCylinder>())
                resultReader.Read(snappedSgc);

            foreach (var snappedBgc in primitives.OfType<SnappedBendedGenCylinder>())
            {
                resultReader.Read(snappedBgc);
            }
        }

        public void Read(double[] values, params SnappedPrimitive[] primitives)
        {
            Read(values, primitives as IEnumerable<SnappedPrimitive>);
        }
    }
}
