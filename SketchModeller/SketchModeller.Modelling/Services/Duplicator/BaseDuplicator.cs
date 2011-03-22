using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.Duplicator
{
    [ContractClass(typeof(BaseDuplicatorContract<,>))]
    abstract class BaseDuplicator<TSnapped, TNew> : IDuplicator
        where TSnapped : SnappedPrimitive
        where TNew : NewPrimitive
    {
        public Type SnappedType
        {
            get { return typeof(TSnapped); }
        }

        public Type NewType
        {
            get { return typeof(TNew); }
        }

        public abstract bool IsNatural { get; }

        public NewPrimitive Duplicate(SnappedPrimitive prim)
        {
            TSnapped snapped = (TSnapped)prim;
            return DuplicateCore(snapped);
        }

        protected abstract TNew DuplicateCore(TSnapped snapped);
    }

    [ContractClassFor(typeof(BaseDuplicator<,>))]
    abstract class BaseDuplicatorContract<TSnapped, TNew> : BaseDuplicator<TSnapped, TNew>
        where TSnapped : SnappedPrimitive
        where TNew : NewPrimitive
    {
        public override bool IsNatural
        {
            get { return false; }
        }

        protected override TNew DuplicateCore(TSnapped snapped)
        {
            Contract.Requires(snapped != null);
            Contract.Ensures(Contract.Result<TNew>() != null);
            return null;
        }
    }
}
