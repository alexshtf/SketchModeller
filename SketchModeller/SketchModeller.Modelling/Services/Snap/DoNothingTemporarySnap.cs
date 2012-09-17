using SketchModeller.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Modelling.Services.Snap
{
    class DoNothingTemporarySnap : ITemporarySnap
    {
        public void Update()
        {
        }

        public void Dispose()
        {
        }
    }
}
