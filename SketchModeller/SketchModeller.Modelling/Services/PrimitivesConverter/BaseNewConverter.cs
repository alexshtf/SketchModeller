using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    [ContractClass(typeof(BaseNewConverterContract<,>))]
    abstract class BaseNewConverter<TSource, TTarget> : INewConverter
        where TSource : NewPrimitive
        where TTarget : NewPrimitive
    {
        public Type SourceType
        {
            get { return typeof(TSource); }
        }

        public Type TargetType
        {
            get { return typeof(TTarget); }
        }

        public NewPrimitive Convert(NewPrimitive source, Vector3D moveVector)
        {
            var concreteSource = (TSource)source;
            return ConvertCore(concreteSource, moveVector);
        }

        public void ApplyMovement(NewPrimitive source, NewPrimitive target, Vector3D moveVector)
        {
            var concreteSource = (TSource)source;
            var concreteTarget = (TTarget)target;
            ApplyMovementCore(concreteSource, concreteTarget, moveVector);
        }

        protected abstract TTarget ConvertCore(TSource source, Vector3D moveVector);

        protected abstract void ApplyMovementCore(TSource source, TTarget target, Vector3D moveVector);
    }

    [ContractClassFor(typeof(BaseNewConverter<,>))]
    abstract class BaseNewConverterContract<TSource, TTarget> : BaseNewConverter<TSource, TTarget>
        where TSource : NewPrimitive
        where TTarget : NewPrimitive
    {

        protected override TTarget ConvertCore(TSource source, Vector3D moveVector)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<TTarget>() != null);
            return null;
        }

        protected override void ApplyMovementCore(TSource source, TTarget target, Vector3D moveVector)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
        }
    }
}
