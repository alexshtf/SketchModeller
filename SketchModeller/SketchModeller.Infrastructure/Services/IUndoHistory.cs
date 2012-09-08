using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

namespace SketchModeller.Infrastructure.Services
{
    public interface IUndoHistory
    {
        void Push();
        void Pop();
    }
}
