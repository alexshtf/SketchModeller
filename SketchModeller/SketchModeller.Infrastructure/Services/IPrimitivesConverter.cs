using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Services
{
    [ContractClass(typeof(PrimitivesConverterContract))]
    public interface IPrimitivesConverter
    {
        /// <summary>
        /// Converts a snapped primitive to a new primitive of the same type, preserving all its properties.
        /// </summary>
        /// <param name="source">The snapped primitive to convert</param>
        /// <returns>The new primitive representing the same primitive as <paramref name="source"/>.</returns>
        NewPrimitive SnappedToNew(SnappedPrimitive source);

        /// <summary>
        /// Converts a new primitive to a new primitive of a different type, preserving as much semantics as possible.
        /// </summary>
        /// <param name="source">The source new primitive to be converted</param>
        /// <param name="targetType">The type of the target new primitive that will be created from the source</param>
        /// <param name="moveVector">The movement vector from the primitive specified by <paramref name="source"/> to the 
        /// target primitive.</param>
        /// <returns></returns>
        NewPrimitive NewToNew(NewPrimitive source, Type targetType, Vector3D moveVector);

        /// <summary>
        /// Applies a movement from the position of a source primitive to the position of a target primitive.
        /// </summary>
        /// <param name="source">The source primitive</param>
        /// <param name="target">The target primitive</param>
        /// <param name="moveVector">The movement vector</param>
        void ApplyMovement(NewPrimitive source, NewPrimitive target, Vector3D moveVector);

        /// <summary>
        /// Gets the valid target types of new primitives that the specified source primitive can be converted to.
        /// </summary>
        /// <param name="source">The source new primitive</param>
        /// <returns></returns>
        [Pure]
        Type[] GetTargetTypes(NewPrimitive source);
    }

    [ContractClassFor(typeof(IPrimitivesConverter))]
    abstract class PrimitivesConverterContract : IPrimitivesConverter
    {
        public NewPrimitive SnappedToNew(SnappedPrimitive source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            return null;
        }

        public NewPrimitive NewToNew(NewPrimitive source, Type targetType, Vector3D moveVector)
        {
            Contract.Requires(source != null);
            Contract.Requires(targetType != null);
            Contract.Requires(GetTargetTypes(source).Contains(targetType));
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            Contract.Ensures(Contract.Result<NewPrimitive>().GetType() == targetType);
            return null;
        }

        public void ApplyMovement(NewPrimitive source, NewPrimitive target, Vector3D moveVector)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Requires(GetTargetTypes(source).Contains(target.GetType()));
        }

        public Type[] GetTargetTypes(NewPrimitive source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<Type[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<Type[]>(), type => type != null));
            Contract.Ensures(Contract.Result<Type[]>().Contains(source.GetType()));
            return null;
        }
    }

}
