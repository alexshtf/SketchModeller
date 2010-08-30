using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace SketchToCyl
{
    class ToolbarCommand : DelegateCommand
    {
        public ToolbarCommand(string title, Action<object> execute)
            : base(execute)
        {
            Title = title;
        }

        public ToolbarCommand(string title, Action<object> execute, Func<object, bool> canExecute)
            : base(execute, canExecute)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
