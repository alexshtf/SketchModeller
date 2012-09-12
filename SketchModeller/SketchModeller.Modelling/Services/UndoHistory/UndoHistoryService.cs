using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Utils;

namespace SketchModeller.Modelling.Services.UndoHistory
{
    class UndoHistoryService : IUndoHistory
    {
        private readonly IUndoStack undoStack;
        private readonly SessionData sessionData;
        private readonly IClassificationInference classificationInference;

        public UndoHistoryService(SessionData sessionData, IClassificationInference classificationInference)
        {
            this.sessionData = sessionData;
            this.classificationInference = classificationInference;
            this.undoStack = new SerializingMemoryUndoStack();
        }

        public void Push()
        {
            if (sessionData.SketchData == null)
                return;

            // synchronize modelling session changed back to SketchData
            sessionData.SketchData.NewPrimitives =
                sessionData.NewPrimitives
                .ToArray();

            sessionData.SketchData.SnappedPrimitives =
                sessionData.SnappedPrimitives
                .ToArray();

            sessionData.SketchData.Annotations =
                sessionData.Annotations
                .ToArray();

            sessionData.SketchData.Curves =
                sessionData.SketchObjects
                .ToArray();

            undoStack.Push(sessionData.SketchData);
        }

        public void Pop()
        {
            var sketchData = undoStack.Pop();
            if (sketchData == null)
                return;

            sessionData.SketchData = sketchData;

            sessionData.NewPrimitives.Clear();
            sessionData.SnappedPrimitives.Clear();
            sessionData.Annotations.Clear();
            sessionData.FeatureCurves.Clear();

            if (sketchData.NewPrimitives != null)
                sessionData.NewPrimitives.AddRange(sketchData.NewPrimitives);
            if (sketchData.SnappedPrimitives != null)
            {
                sessionData.SnappedPrimitives.AddRange(sketchData.SnappedPrimitives);
                sessionData.FeatureCurves.AddRange(sketchData.SnappedPrimitives.SelectMany(sp => sp.FeatureCurves));
            }
            if (sketchData.Annotations != null)
                sessionData.Annotations.AddRange(sketchData.Annotations);

            var curves = sketchData.Curves ?? System.Linq.Enumerable.Empty<PointsSequence>();
            sessionData.SketchObjects = curves.ToArray();
            foreach (var item in sessionData.SketchObjects)
                item.ColorCodingIndex = PointsSequence.INVALID_COLOR_CODING;
        }

        public void Clear()
        {
            undoStack.Clear();
        }
    }
}
