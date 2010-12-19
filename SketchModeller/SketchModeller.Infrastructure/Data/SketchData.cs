using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    public class SketchData
    {
        [XmlIgnoreAttribute]
        public double[,] Image { get; set; }
        public Point[] Points { get; set; }
        public NewCylinder[] Cylinders { get; set; }
    }
}
