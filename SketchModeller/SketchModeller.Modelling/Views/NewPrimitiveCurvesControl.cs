using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Controls;
using Utils;

using Polyline = System.Windows.Shapes.Polyline;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace SketchModeller.Modelling.Views
{
    class NewPrimitiveCurvesControl : ContentControl
    {
        private readonly Grid grid;

        public NewPrimitiveCurvesControl()
        {
            grid = new Grid();
            Content = grid;
        }

        #region Primitive dependency property

        public static readonly DependencyProperty PrimitiveProperty =
            DependencyProperty.Register("Primitive", typeof(NewPrimitive), typeof(NewPrimitiveCurvesControl), new FrameworkPropertyMetadata(OnPrimitiveChanged));

        public NewPrimitive Primitive
        {
            get { return (NewPrimitive)GetValue(PrimitiveProperty); }
            set { SetValue(PrimitiveProperty, value); }
        }

        private static void OnPrimitiveChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var control = (NewPrimitiveCurvesControl)source;
            control.OnPrimitiveChanged();
        }

        #endregion

        private void OnPrimitiveChanged()
        {
            Update();
        }

        public void Update()
        {
            grid.Children.Clear();
            
            if (Primitive == null)
                return;

            var featureInfos =
                from c in Primitive.FeatureCurves
                select new { Curve = c, Brush = Brushes.Orange };

            var silhouetteInfos =
                from c in Primitive.SilhouetteCurves
                select new { Curve = c, Brush = Brushes.DarkOrange };

            var allCurveInfos = featureInfos.Concat(silhouetteInfos);
            foreach (var curveInfo in allCurveInfos)
            {
                // create curve stroke
                var path = CreatePolyline(curveInfo.Curve.Points);
                path.Stroke = curveInfo.Brush;
                path.StrokeThickness = 2;
                grid.Children.Add(path);

                if (curveInfo.Curve.AssignedTo != null)
                {
                    // create perpendicular line towards the assigned curve
                    var fstPoint = curveInfo.Curve.ClosestPoint;
                    var sndPoint = fstPoint.ProjectionOnCurve(curveInfo.Curve.AssignedTo.Points).Item1;
                    var line = CreatePolyline(new Point[] { fstPoint, sndPoint });
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 1;
                    line.StrokeDashArray = new DoubleCollection(new double[] { 5, 5 });

                    grid.Children.Add(line);
                }
            }
        }

        private Path CreatePolyline(IEnumerable<Point> points)
        {
            var geometry = new StreamGeometry();
            if (points != null && points.Any())
            {
                var ctx = geometry.Open();
                ctx.BeginFigure(points.First(), false, false);
                ctx.PolyLineTo(points.Skip(1).ToList(), true, true);
                ctx.Close();
            }
            
            var scaleTransform = new ScaleTransform();
            scaleTransform.Bind(ScaleTransform.ScaleXProperty, () => this.ActualWidth, width => width / 2);
            scaleTransform.Bind(ScaleTransform.ScaleYProperty, () => this.ActualHeight, height => height / 2);

            var translateTransform = new TranslateTransform();
            translateTransform.Bind(TranslateTransform.XProperty, () => this.ActualWidth, width => width / 2);
            translateTransform.Bind(TranslateTransform.YProperty, () => this.ActualHeight, height => height / 2);

            var transformGroup = new TransformGroup { Children = { scaleTransform, translateTransform } };
            geometry.Transform = transformGroup;

            var path = new Path { Data = geometry };
            return path;
        }
    }
}
