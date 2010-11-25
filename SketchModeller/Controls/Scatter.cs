using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace Controls
{
    public class Scatter : Shape
    {
        #region Points property

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(PointCollection), typeof(Scatter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange));


        #endregion

        protected override Geometry DefiningGeometry
        {
            get 
            {
                if (Points == null)
                    return Geometry.Empty;

                var result = new GeometryGroup();
                foreach (var point in Points)
                {
                    var pointGeometry = new EllipseGeometry(point, 2, 2);
                    result.Children.Add(pointGeometry);
                }

                return result;
            }
        }
    }
}
