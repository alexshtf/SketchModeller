using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Services
{
    /// <summary>
    /// Provides ability to duplicate a snapped primitive to create a (possibly different) new primitive.
    /// </summary>
    [ContractClass(typeof(PrimitivesDuplicatorContract))]
    public interface IPrimitivesDuplicator
    {
        /// <summary>
        /// Duplicates a snapped primitive to create a new primitive from it. Attempts to transfer properties
        /// from the snapped to the new primitives in a meaningful way.
        /// </summary>
        /// <param name="snapped">The snapped primitive to copy properties from</param>
        /// <param name="targetType">The type of the new primitive to create</param>
        /// <returns>The duplicated new primitive</returns>
        NewPrimitive Duplicate(SnappedPrimitive snapped, Type targetType);

        /// <summary>
        /// Gets a list of types of <c>NewPrimitive</c> objects the given snapped primitive
        /// can be duplicated to.
        /// </summary>
        /// <param name="snapped">The given snapped primitive</param>
        /// <returns>An array of types of <c>NewPrimitive</c> objects that <paramref name="snappedPrimitive"/>
        /// can be duplicated to.</returns>
        [Pure] 
        Type[] GetValidDuplicateTypes(SnappedPrimitive snapped);

        /// <summary>
        /// Gets the natural type of <c>NewPrimitive</c> object that the given snapped primitive
        /// can be duplicated to.
        /// </summary>
        /// <param name="snapped">The snapped primitive to duplicate.</param>
        /// <returns>The type of the most natural new primitive that <paramref name="snapped"/> can be duplicated to. Usually
        /// it is just the same primitive in its "new primitive" version.</returns>
        [Pure]
        Type GetNaturalDuplicateType(SnappedPrimitive snapped);
    }

    [ContractClassFor(typeof(IPrimitivesDuplicator))]
    abstract class PrimitivesDuplicatorContract : IPrimitivesDuplicator
    {
        public NewPrimitive Duplicate(SnappedPrimitive snapped, Type targetType)
        {
            Contract.Requires(snapped != null);
            Contract.Requires(targetType != null);
            Contract.Requires(typeof(NewPrimitive).IsAssignableFrom(targetType));
            Contract.Requires(Contract.Exists(GetValidDuplicateTypes(snapped), type => type == targetType));
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            Contract.Ensures(Contract.Result<NewPrimitive>().GetType() == targetType);
            return default(NewPrimitive);
        }

        public Type[] GetValidDuplicateTypes(SnappedPrimitive snapped)
        {
            Contract.Requires(snapped != null);
            Contract.Ensures(Contract.Result<Type[]>() != null);
            Contract.Ensures(Contract.Result<Type[]>().Length > 0); // every snapped primitive is at least convertible to its new counterpart
            Contract.Ensures(Contract.ForAll(Contract.Result<Type[]>(), type => typeof(NewPrimitive).IsAssignableFrom(type)));
            return null;
        }

        public Type GetNaturalDuplicateType(SnappedPrimitive snapped)
        {
            Contract.Requires(snapped != null);
            Contract.Ensures(Contract.Result<Type>() != null);
            Contract.Ensures(Contract.Exists(GetValidDuplicateTypes(snapped), type => type == Contract.Result<Type>()));
            return null;
        }
    }

}
