using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

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

        [Pure]
        public bool HasAnnotation<T>()
        {
            return Annotations.OfType<T>().FirstOrDefault() != null;
        }

        [Pure]
        public IEnumerable<T> GetAnnotations<T>()
        {
            return Annotations.OfType<T>();
        }
    }
}
