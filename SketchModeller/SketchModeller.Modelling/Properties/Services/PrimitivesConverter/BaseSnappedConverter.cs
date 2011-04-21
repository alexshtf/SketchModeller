using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    [ContractClass(typeof(BaseSnappedConverterContract<>))]
    abstract class BaseSnappedConverter<TSnapped>: ISnappedConverter
        where TSnapped : SnappedPrimitive
    {
        public Type SnappedType
        {
            get { return typeof(TSnapped); }
        }

        public NewPrimitive Convert(SnappedPrimitive snapped)
        {
            var concreteSnapped = (TSnapped)snapped;
            return ConvertCore(concreteSnapped);
        }

        /// <summary>
        /// A type safe method for performing the conversion.
        /// </summary>
        /// <param name="snapped">The snapped primitive to be converter</param>
        /// <returns></returns>
        protected abstract NewPrimitive ConvertCore(TSnapped snapped);
    }

    [ContractClassFor(typeof(BaseSnappedConverter<>))]
    abstract class BaseSnappedConverterContract<TSnapped> : BaseSnappedConverter<TSnapped>
        where TSnapped : SnappedPrimitive
    {
        protected override NewPrimitive ConvertCore(TSnapped snapped)
        {
            Contract.Requires(snapped != null);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null);
            return null;
        }
    }

}
