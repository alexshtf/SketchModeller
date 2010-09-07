using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiviewCurvesToCyl.Persistency
{
    [Serializable]
    class State
    {
        public Curve[] Curves { get; set; }
    }
}
