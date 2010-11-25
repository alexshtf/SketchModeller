using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Views
{
    public class DisplayOptionsViewModel
    {
        public DisplayOptionsViewModel()
        {
        }

        public DisplayOptionsViewModel(DisplayOptions displayOptions)
            : this()
        {
            DisplayOptions = displayOptions;
        }

        public DisplayOptions DisplayOptions
        {
            get;
            private set;
        }
    }
}
