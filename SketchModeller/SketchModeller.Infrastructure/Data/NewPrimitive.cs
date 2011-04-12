using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class NewPrimitive : SelectablePrimitive
    {
        private PrimitiveCurve[] featureCurves;
        public PrimitiveCurve[] FeatureCurves
        {
            get { return featureCurves; }
            protected set { featureCurves = value; }
        }

        private PrimitiveCurve[] silhouetteCurves;
        public PrimitiveCurve[] SilhouetteCurves
        {
            get { return silhouetteCurves; }
            protected set { silhouetteCurves = value; }
        }

        public abstract void UpdateCurvesGeometry();
    }
}
