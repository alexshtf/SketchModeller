using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Infrastructure.Services
{
    public interface ITemporarySnap : IDisposable
    {
        void Update();
    }

    public interface ISnapper
    {
        ITemporarySnap TemporarySnap(NewPrimitive primitive);
        void Snap();
        void Recalculate();
    }
}
