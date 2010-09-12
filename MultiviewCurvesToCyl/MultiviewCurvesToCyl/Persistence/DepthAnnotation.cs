using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    [DataContract]
    class DepthAnnotation : BaseAnnotation
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public double Depth { get; set; }
    }
}
