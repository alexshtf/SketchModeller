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
            return new PrimitivesReader();
        }

        public IPrimitivesWriter CreateWriter()
        {
            return new PrimitivesWriter();
        }
    }
}
