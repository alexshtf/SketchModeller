using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    /// <summary>
    /// Converts snapped primitives to their corresponding new primitives
    /// </summary>
    [ContractClass(typeof(SnappedConverterContract))]
    interface ISnappedConverter
    {
        Type SnappedType 
        { 
            get; 
        }

        NewPrimitive Convert(SnappedPrimitive snapped);
    }

    [ContractClassFor(typeof(ISnappedConverter))]
    abstract class SnappedConverterContract : ISnappedConverter
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

        public NewPrimitive Convert(SnappedPrimitive snapped)
        {
            Contract.Requires(snapped != null);
            Contract.Requires(snapped.GetType() == SnappedType);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            return null;
        }
    }

}
