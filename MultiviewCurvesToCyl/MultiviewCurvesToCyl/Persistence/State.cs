using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    [DataContract]
    class State
    {
        [DataMember]
        public Curve[] Curves { get; set; }
    }
}
