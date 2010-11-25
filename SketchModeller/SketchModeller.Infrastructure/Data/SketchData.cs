using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class SketchData
    {
        public double[,] Image { get; set; }
        public Point[] Points { get; set; }
    }
}
