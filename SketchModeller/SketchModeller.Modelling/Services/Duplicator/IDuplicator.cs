using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Duplicator
{
    [ContractClass(typeof(DuplicatorContract))]
    interface IDuplicator
    {
        /// <summary>
        /// The type of the snapped primitives this duplicator is expecting in its <see cref="Duplicate"/> method.
        /// </summary>
        Type SnappedType { get; }

        /// <summary>
        /// The type of the new primitive this duplicator creates in its <see cref="Duplicate"/> method.
        /// </summary>
        Type NewType { get; }

        /// <summary>
        /// A flag specifying weather this duplicator is a "natural" one. That is, converting a snapped primitive to a new
        /// primitive of the same type.
        /// </summary>
        bool IsNatural { get; }

        /// <summary>
        /// Performs duplication of data from snapped to new primitives.
        /// </summary>
        /// <param name="prim">The snapped primitive to duplicate.</param>
        /// <returns>A new primitive that has similar semantics like the given snapped primitive.</returns>
        NewPrimitive Duplicate(SnappedPrimitive prim);
    }

    [ContractClassFor(typeof(IDuplicator))]
    abstract class DuplicatorContract : IDuplicator
    {
        public Type SnappedType
        {
            get 
            { 
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(typeof(SnappedPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null;
            }
        }

        public Type NewType
        {
            get 
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(typeof(NewPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null;
            }
        }

        public bool IsNatural
        {
            get { return false; }
        }

        public NewPrimitive Duplicate(SnappedPrimitive prim)
        {
            Contract.Requires(prim != null);
            Contract.Requires(prim.GetType() == SnappedType);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            Contract.Ensures(Contract.Result<NewPrimitive>().GetType() == NewType);
            return null;
        }
    }

}
