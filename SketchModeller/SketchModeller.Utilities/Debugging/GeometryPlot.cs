using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Debugging
{
    public static class GeometryPlot
    {
        public static GeometriesPlotter Create(double width, double height)
        {
            return new GeometriesPlotter(width, height);
        }
    }
}
