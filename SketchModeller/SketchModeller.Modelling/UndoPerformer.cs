using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
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
            
            var undoEvent = eventAggregator.GetEvent<UndoEvent>();
            undoEvent.Subscribe(_ => OnUndo());

            var globalShortcut = eventAggregator.GetEvent<GlobalShortcutEvent>();
            globalShortcut.Subscribe(eventArgs =>
                                         {
                                             if (eventArgs.Key == Key.Z &&
                                                 eventArgs.KeyboardDevice.Modifiers == ModifierKeys.Control)
                                                 OnUndo();
                                         });
        }

        private  void OnUndo()
        {
            undoHistory.Pop();
        }
    }
}
