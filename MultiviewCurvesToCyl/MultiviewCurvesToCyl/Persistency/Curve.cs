using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MultiviewCurvesToCyl.Persistency
{
    [Serializable]
    class Curve
    {
        public Point[] Points { get; set; }
    }
}
