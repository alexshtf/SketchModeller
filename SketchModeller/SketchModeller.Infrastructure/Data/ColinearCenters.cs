﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class ColinearCenters : Annotation
    {
        public FeatureCurve[] Elements { get; set; }
    }
}
