using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class OnSphere : Annotation
    {
        public FeatureCurve SphereOwned { get; set; }
        public FeatureCurve CenterTouchesSphere { get; set; }
    }
}
