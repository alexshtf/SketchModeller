using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SketchData
    {
        public PointsSequence[] Curves { get; set; }
        public double[][,] DistanceTransforms { get; set; }
        public NewPrimitive[] NewPrimitives { get; set; }
        public SnappedPrimitive[] SnappedPrimitives { get; set; }
        public Annotation[] Annotations { get; set; }
    }
}
