using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Utils;

namespace SketchModeller.Modelling.Views
{
    public partial class SketchView
    {
        public static readonly IValueConverter MouseModeToStringConverter =
            new DelegateConverter<MouseInterationModes>(mode =>
            {
                switch (mode)
                {
                    case MouseInterationModes.CurveSelection:
                        return "Curve";
                    case MouseInterationModes.PrimitiveManipulation:
                        return "Primitive";
                    case MouseInterationModes.FeatureCurveSelection:
                        return "Feature";
                    default:
                        throw new NotSupportedException();
                }
            });
    }
}
