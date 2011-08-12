using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class NewPrimitive : SelectablePrimitive
    {
        public PrimitiveEditConstraint[] EditConstraints { get; set; }

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

        public PrimitiveCurve[] AllCurves
        {
            get
            {
                var featuresOrEmpty = featureCurves == null ? Enumerable.Empty<PrimitiveCurve>() : featureCurves;
                var silhouettesOrEmpty = silhouetteCurves == null ? Enumerable.Empty<PrimitiveCurve>() : silhouetteCurves;
                return featuresOrEmpty.Concat(silhouettesOrEmpty).ToArray();
            }
        }

        public abstract void UpdateCurvesGeometry();
    }
}
