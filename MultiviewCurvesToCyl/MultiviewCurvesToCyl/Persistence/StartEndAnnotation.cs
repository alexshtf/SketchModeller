using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    [DataContract]
    class StartEndAnnotation : BaseAnnotation
    {
        [DataMember]
        public int StartIndex { get; set; }

        [DataMember]
        public int EndIndex { get; set; }
    }
}
