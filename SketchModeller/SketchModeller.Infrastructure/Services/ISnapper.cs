using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Reactive;

namespace SketchModeller.Infrastructure.Services
{
    public interface ITemporarySnap : IDisposable
    {
        void Update();
    }

    public interface ISnapper
    {
        ITemporarySnap TemporarySnap(NewPrimitive primitive);
        IObservable<Unit> SnapAsync();
        IObservable<Unit> RecalculateAsync();
    }
}
