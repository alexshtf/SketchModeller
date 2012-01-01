using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    /// <summary>
    /// Writes current values and variables of snapped primitives and allows getting the current
    /// array of variables and their corresponding values.
    /// </summary>
    /// <example>
    /// The following code demonstrates the typical usage pattern.
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
    interface IPrimitivesWriter
    {
        /// <summary>
        /// Gets an array of all the variables of the currently written primitives.
        /// </summary>
        /// <returns>An array of variables.</returns>
        /// <remarks>The length of this array is the same as the array returned from <see cref="GetValues"/> and the item
        /// at index <c>i</c> of this function's array corresponds to the item at the same index in the array of <see cref="GetValues"/></remarks>
        Variable[] GetVariables();

        /// <summary>
        /// Gets an array of all the values of the currently written primitives.
        /// </summary>
        /// <returns>An array of the current values</returns>
        /// <remarks>The length of this array is the same as the array returned from <see cref="GetVariables"/> and the item
        /// at index <c>i</c> of this function's array corresponds to the item at the same index in the array of <see cref="GetVariables"/></remarks>
        double[] GetValues();

        /// <summary>
        /// Writes the values and the variables of each primitive in the given array.
        /// </summary>
        /// <param name="primitives">The array of primitives</param>
        void Write(params SnappedPrimitive[] primitives);

        /// <summary>
        /// Writes the values and the variables of each primitive in the given collection.
        /// </summary>
        /// <param name="primitives">The collection of primitives.</param>
        void Write(IEnumerable<SnappedPrimitive> primitives);
    }
}
