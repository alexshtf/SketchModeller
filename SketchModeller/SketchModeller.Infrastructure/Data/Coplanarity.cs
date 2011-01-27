using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class Coplanarity : Annotation
    {
        public PointsSequence[] Elements { get; set; }
    }
}
