using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.UndoHistory
{
    interface IUndoStack
    {
        void Push(SketchData sketchData);
        SketchData Pop();
        void Clear();
    }
}
