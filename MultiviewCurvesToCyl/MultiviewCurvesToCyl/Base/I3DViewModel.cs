using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MultiviewCurvesToCyl.Base
{
    interface I3DViewModel : INotifyPropertyChanged
    {
        bool IsInWireframeMode { get; set; }
    }
}
