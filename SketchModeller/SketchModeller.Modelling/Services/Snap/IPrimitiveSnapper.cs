using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Infrastructure.Shared;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.Snap
{
    /// <summary>
    /// Represents a primitives snapper. Knows how to snap specific kinds of primitives.
    /// </summary>
    [ContractClass(typeof(PrimitiveSnapperContract))]
    interface IPrimitiveSnapper
    {
        /// <summary>
        /// Initializes the primitive snapped with the relevant data. Should be called by the snappers manager.
        /// </summary>
        /// <param name="uiState">The shared UiState object the snapper can use.</param>
        /// <param name="sessionData">The shared session data object the snapper can use</param>
        void Initialize(UiState uiState, SessionData sessionData);

        /// <summary>
        /// Creates a snapped primitive given a new (temporary) primitive.
        /// </summary>
        /// <param name="newPrimitive">The new primitive selected for snapping</param>
        /// <returns>A <c>SnappedPrimitive</c> object that represents the snapped version of this primitive.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a snapped primitive cannot be created because of
        /// missing or invalid input.</exception>
        SnappedPrimitive Create(NewPrimitive newPrimitive);

        /// <summary>
        /// Creates the objective term and constraint terms that are used to reconstruct the primitive given its curves.
        /// </summary>
        /// <param name="snappedPrimitive">The snapped primitive to reconstruct</param>
        /// <returns>A pair containing the objective function and the constraints such that their optimization reconstructs
        /// the pimitive's parameters.</returns>
        Tuple<Term, Term[]> Reconstruct(SnappedPrimitive snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations);

        /// <summary>
        /// The type of the <see cref="NewPrimitive"/> object this snapper can handle.
        /// </summary>
        Type NewPrimitiveType { get; }

        /// <summary>
        /// The type of the <see cref="SnappedPrimitive"/> objects this snapper creates and reconstructs.
        /// </summary>
        Type SnappedPrimitiveType { get; }
    }

    [ContractClassFor(typeof(IPrimitiveSnapper))]
    abstract class PrimitiveSnapperContract : IPrimitiveSnapper
    {
        void IPrimitiveSnapper.Initialize(UiState uiState, SessionData sessionData)
        {
            Contract.Requires(uiState != null);
            Contract.Requires(sessionData != null);
        }

        SnappedPrimitive IPrimitiveSnapper.Create(NewPrimitive newPrimitive)
        {
            Contract.Requires(newPrimitive != null);
            Contract.Requires(NewPrimitiveType.IsAssignableFrom(newPrimitive.GetType()));
            
            Contract.Ensures(Contract.Result<SnappedPrimitive>() != null);
            Contract.Ensures(SnappedPrimitiveType.IsAssignableFrom(Contract.Result<SnappedPrimitive>().GetType()));

            return null;
        }

        Tuple<Term, Term[]> IPrimitiveSnapper.Reconstruct(SnappedPrimitive snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            Contract.Requires(snappedPrimitive != null);
            Contract.Requires(SnappedPrimitiveType.IsAssignableFrom(snappedPrimitive.GetType()));
            Contract.Requires(curvesToAnnotations != null);
            Contract.Requires(Contract.ForAll(curvesToAnnotations, pair => pair.Value != null));

            Contract.Ensures(Contract.Result<Tuple<Term, Term[]>>() != null);
            Contract.Ensures(Contract.Result<Tuple<Term, Term[]>>().Item1 != null);
            Contract.Ensures(Contract.Result<Tuple<Term, Term[]>>().Item2 != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<Tuple<Term, Term[]>>().Item2, constraint => constraint != null));
            return null;
        }

        public Type NewPrimitiveType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(typeof(NewPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null;
            }
        }


        public Type SnappedPrimitiveType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(typeof(SnappedPrimitive).IsAssignableFrom(Contract.Result<Type>()));
                return null; 
            }
        }
    }

}
