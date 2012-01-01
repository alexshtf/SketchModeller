using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    class TemporarySnap : ITemporarySnap
    {
        private readonly SessionData sessionData;
        private readonly SnappersManager snappersManager;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly SnappedPrimitive snappedPrimitive;

        public TemporarySnap(SessionData sessionData,
                             SnappersManager snappersManager,
                             PrimitivesReaderWriterFactory primitivesReaderWriterFactory,
                             IEventAggregator eventAggregator,
                             NewPrimitive newPrimitive)
        {
            this.sessionData = sessionData;
            this.snappersManager = snappersManager;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
            this.eventAggregator = eventAggregator;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
