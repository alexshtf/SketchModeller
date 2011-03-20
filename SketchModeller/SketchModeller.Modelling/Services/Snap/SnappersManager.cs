using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Data;
using AutoDiff;

namespace SketchModeller.Modelling.Services.Snap
{
    /// <summary>
    /// Managers a registry of snappers, and can perform snapping operations on new/snapped primitives base on their type
    /// using the appropriate snapper.
    /// </summary>
    class SnappersManager
    {
        private readonly List<IPrimitiveSnapper> snappers;
        private readonly UiState uiState;
        private readonly SessionData sessionData;

        public SnappersManager(UiState uiState, SessionData sessionData)
        {
            Contract.Requires(uiState != null);
            Contract.Requires(sessionData != null);

            this.snappers = new List<IPrimitiveSnapper>();
            this.uiState = uiState;
            this.sessionData = sessionData;
        }

        public void RegisterSnapper(IPrimitiveSnapper snapper)
        {
            snapper.Initialize(uiState, sessionData);
            snappers.Add(snapper);
        }

        public SnappedPrimitive Create(PointsSequence[] selectedCurves, NewPrimitive newPrimitive)
        {
            Contract.Requires(selectedCurves != null);
            Contract.Requires(Contract.ForAll(selectedCurves, c => c != null));
            Contract.Requires(newPrimitive != null);
            Contract.Ensures(Contract.Result<SnappedPrimitive>() != null);

            // find appropriate type snapper
            var newPrimitiveType = newPrimitive.GetType();
            var snapper = 
                snappers
                .Where(s => s.NewPrimitiveType.IsAssignableFrom(newPrimitiveType))
                .FirstOrDefault();

            if (snapper == null)
                throw new InvalidOperationException("Cannot find snapper that can snap new primitives of type " + newPrimitiveType);

            // return the snapper's result
            return snapper.Create(selectedCurves, newPrimitive);
        }

        public Tuple<Term, Term[]> Reconstruct(SnappedPrimitive snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            // find appropriate type snapper
            var snappedPrimitiveType = snappedPrimitive.GetType();
            var snapper =
                snappers
                .Where(s => s.SnappedPrimitiveType.IsAssignableFrom(snappedPrimitiveType))
                .FirstOrDefault();

            if (snapper == null)
                throw new InvalidOperationException("Cannot find snapper that can snap primitives of type " + snappedPrimitiveType);

            return snapper.Reconstruct(snappedPrimitive, curvesToAnnotations);
        }
    }
}
