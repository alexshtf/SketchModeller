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
        public Polyline[] Polylines { get; set; }
        public Polygon[] Polygons { get; set; }
        public NewPrimitive[] NewPrimitives { get; set; }
        public SnappedPrimitive[] SnappedPrimitives { get; set; }
        public Annotation[] Annotations { get; set; }
    }
}
