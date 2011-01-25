using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    public class SketchData
    {
        [XmlIgnore]
        public Point[] Points { get; set; }
        [XmlIgnore]
        public Polyline[] Polylines { get; set; }
        [XmlIgnore]
        public Polygon[] Polygons { get; set; }

        [XmlArrayItem(typeof(NewCylinder))]
        [XmlArrayItem(typeof(NewHalfSphere))]
        public NewPrimitive[] NewPrimitives { get; set; }
        
        // snapped data
        [XmlArrayItem(typeof(SnappedCylinder))]
        [XmlArrayItem(typeof(SnappedHalfSphere))]
        public SnappedPrimitive[] SnappedPrimitives { get; set; }

        // annotations on the curves.
        [XmlArrayItem(typeof(Coplanarity))]
        [XmlArrayItem(typeof(Parallelism))]
        public Annotation[] Annotations { get; set; }
    }
}
