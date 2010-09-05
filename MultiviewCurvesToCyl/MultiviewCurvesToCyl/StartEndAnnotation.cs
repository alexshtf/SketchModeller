using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiviewCurvesToCyl
{
    class StartEndAnnotation : ICurveAnnotation
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
