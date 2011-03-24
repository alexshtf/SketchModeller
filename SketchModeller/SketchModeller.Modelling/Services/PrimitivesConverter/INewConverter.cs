using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    /// <summary>
    /// Converts new primitives to new primitives of a different type
    /// </summary>
    [ContractClass(typeof(NewConverterContract))]
    interface INewConverter
    {
        /// <summary>
        /// Type of the source <c>NewPrimitive</c> objects this converter accepts.
        /// </summary>
        Type SourceType
        {
            get;
        }

        /// <summary>
        /// The type of the target <c>NewPrimitive</c> objects this converter accepts.
        /// </summary>
        Type TargetType
        {
            get;
        }

        /// <summary>
        /// Performs conversion of a source new primitive to another type, after being moved according to a given vector.
        /// </summary>
        /// <param name="source">The source <c>NewPrimitive</c> object</param>
        /// <param name="moveVector">The movement vector of the source primitive</param>
        /// <returns>Another <c>NewPrimitive</c> object generated from the data in <paramref name="source"/> and has
        /// the semantics of being moved according to <paramref name="moveVector"/>.</returns>
        NewPrimitive Convert(NewPrimitive source, Vector3D moveVector);
    }

    [ContractClassFor(typeof(INewConverter))]
    abstract class NewConverterContract : INewConverter
    {

        public Type SourceType
        {
            get 
            {
                Contract.Ensures(typeof(NewPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null;
            }
        }

        public Type TargetType
        {
            get 
            {
                Contract.Ensures(typeof(NewPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null;
            }
        }

        public NewPrimitive Convert(NewPrimitive source, Vector3D moveVector)
        {
            Contract.Requires(source != null && source.GetType() == SourceType);
            Contract.Ensures(Contract.Result<NewPrimitive>() != null && Contract.Result<NewPrimitive>().GetType() == TargetType);
            return null;
        }
    }
}
