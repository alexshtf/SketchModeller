using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class RectangleFeatureCurve : FeatureCurve
    {
        public TVec Widgth { get; set; }
        public TVec Height { get; set; }
        public double WidthResult { get; set; }
        public double HeightResult { get; set; }
    }
}
