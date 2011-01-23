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

        // modelling data
        public NewCylinder[] Cylinders { get; set; }

        // snapped data
        [XmlArrayItem(typeof(SnappedCylinder))]
        public SnappedPrimitive[] SnappedPrimitives { get; set; }
    }
}
