using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    [DataContract]
    [KnownType(typeof(DepthAnnotation))]
    [KnownType(typeof(StartEndAnnotation))]
    class Curve
    {
        [DataMember]
        public Point[] Points { get; set; }

        [DataMember]
        public BaseAnnotation[] Annotations { get; set; }
    }
}
