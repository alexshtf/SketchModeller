using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    [DataContract]
    class Point
    {
        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }
    }
}
