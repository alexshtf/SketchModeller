using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace MultiviewCurvesToCyl
{
    class SketchCurve
    {
        public SketchCurve(IEnumerable<Point> polylinePoints)
        {
            PolylinePoints = polylinePoints.ToList().AsReadOnly();
            Annotations = new ObservableCollection<ICurveAnnotation>();
        }
        
        public ReadOnlyCollection<Point> PolylinePoints { get; private set; }
        public ObservableCollection<ICurveAnnotation> Annotations { get; set; }
    }
}
