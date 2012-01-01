using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    /// <summary>
    /// Reads an array of values back to their corresponding fields in snapped primitives.
    /// </summary>
    /// <example>
    /// <code>
    /// var primitivesWriter = GetPrimitivesWriter();
    /// SnappedPrimitive[] snappedPrimitives = GetSnappedPrimitives();
    /// primitivesWriter.Write(snappedPrimitivesCollection);
    /// 
    /// Variable[] variables = primitivesWriter.GetVariables();
    /// double[] values = primitivesWriter.GetValues();
    /// 
    /// double[] optimum = PerformOptimization(objectiveFunction, variables, values);
    /// 
    /// var primitivesReader = GetPrimitivesReader();
    /// primitivesReader.Read(optimum, snappedPrimitives);
    /// </code>
    /// </example>
    interface IPrimitivesReader
    {
        /// <summary>
        /// Reads values from the given array to fields in the given primitives.
        /// </summary>
        /// <param name="values">The values array</param>
        /// <param name="primitives">The collection of primitives</param>
        void Read(double[] values, IEnumerable<SnappedPrimitive> primitives);

        /// <summary>
        /// Reads values from the given array to fields in the given primitives.
        /// </summary>
        /// <param name="values">The values array</param>
        /// <param name="primitives">The array of primitives</param>
        void Read(double[] values, params SnappedPrimitive[] primitives);
    }
}
