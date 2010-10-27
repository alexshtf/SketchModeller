using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace MultiviewCurvesToCyl
{
    abstract class BaseMenuViewModel : BaseViewModel
    {
        public BaseMenuViewModel(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
