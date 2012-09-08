using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Services;

namespace SketchModeller.Modelling
{
    class UndoPerformer
    {
        private readonly IUndoHistory undoHistory;

        public UndoPerformer(IEventAggregator eventAggregator, IUndoHistory undoHistory)
        {
            this.undoHistory = undoHistory;
            var ev = eventAggregator.GetEvent<UndoEvent>();
            ev.Subscribe(_ => OnUndo());
        }

        private  void OnUndo()
        {
            undoHistory.Pop();
        }
    }
}
