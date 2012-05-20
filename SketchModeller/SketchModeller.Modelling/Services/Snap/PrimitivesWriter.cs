using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Utilities;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using System.Diagnostics;
namespace SketchModeller.Modelling.Services.Snap
{
    class PrimitivesWriter : IPrimitivesWriter
    {
        private VariableVectorsWriter variablesWriter;
        private VectorsWriter valuesWriter;

        public PrimitivesWriter()
        {
            variablesWriter = new VariableVectorsWriter();
            valuesWriter = new VectorsWriter();
        }

        public Variable[] GetVariables()
        {
            return variablesWriter.ToArray();
        }

        public double[] GetValues()
        {
            return valuesWriter.ToArray();
        }

        public void Write(params SnappedPrimitive[] primitives)
        {
            Write(primitives as IEnumerable<SnappedPrimitive>);
        }

        public void Write(IEnumerable<SnappedPrimitive> primitives)
        {
            // write cylinders
            foreach (var snappedCylinder in primitives.OfType<SnappedCylinder>())
            {
                variablesWriter.Write(snappedCylinder);
                valuesWriter.Write(snappedCylinder);
            }

            // write cones
            foreach (var snappedCone in primitives.OfType<SnappedCone>())
            {
                variablesWriter.Write(snappedCone);
                valuesWriter.Write(snappedCone);
            }

            // write spheres
            foreach (var snappedSphere in primitives.OfType<SnappedSphere>())
            {
                variablesWriter.Write(snappedSphere);
                valuesWriter.Write(snappedSphere);
            }

            foreach (var snappedSgc in primitives.OfType<SnappedStraightGenCylinder>())
            {
                variablesWriter.Write(snappedSgc);
                valuesWriter.Write(snappedSgc);
            }

            foreach (var snappedCuboid in primitives.OfType<SnappedCuboid>())
            {
                variablesWriter.Write(snappedCuboid);
                valuesWriter.Write(snappedCuboid);
            }

            foreach (var snappedBgc in primitives.OfType<SnappedBendedGenCylinder>())
            {
                variablesWriter.Write(snappedBgc);
                valuesWriter.Write(snappedBgc);
            }
        }
    }
}
