using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Modelling.Services.Snap
{
    class PrimitivesReaderWriterFactory
    {
        public IPrimitivesReader CreateReader()
        {
            throw new NotImplementedException();
        }

        public IPrimitivesWriter CreateWriter()
        {
            return new PrimitivesWriter();
        }
    }
}
