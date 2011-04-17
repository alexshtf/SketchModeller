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
using System.Windows.Input;
using SketchModeller.Infrastructure;

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

        #region PrimitiveCurve attached property

        public static readonly DependencyProperty PrimitiveCurveProperty =
            DependencyProperty.RegisterAttached("PrimitiveCurve", typeof(PrimitiveCurve), typeof(NewPrimitiveCurvesControl));

        public static void SetPrimitiveCurve(FrameworkElement target, PrimitiveCurve value)
        {
            target.SetValue(PrimitiveCurveProperty, value);
        }

        public static PrimitiveCurve GetPrimitiveCurve(FrameworkElement target)
        {
            return (PrimitiveCurve)target.GetValue(PrimitiveCurveProperty);
        }

        #endregion

        #region IsEmphasized attached property

        public static readonly DependencyProperty IsEmphasizedProperty =
            DependencyProperty.RegisterAttached("IsEmphasized", typeof(bool), typeof(NewPrimitiveCurvesControl));

        public static void SetIsEmphasized(FrameworkElement target, bool value)
        {
            target.SetValue(IsEmphasizedProperty, value);
        }

        public static bool GetIsEmphasized(FrameworkElement target)
        {
            return (bool)target.GetValue(IsEmphasizedProperty);
        }

        #endregion

        #region ColorCodingIndex

        public static readonly DependencyProperty ColorCodingIndexProperty =
            DependencyProperty.RegisterAttached("ColorCodingIndex", typeof(int), typeof(NewPrimitiveCurvesControl));

        public static void SetColorCodingIndex(FrameworkElement target, int value)
        {
            target.SetValue(ColorCodingIndexProperty, value);
        }

        public static int GetColorCodingIndex(FrameworkElement target)
        {
            return (int)target.GetValue(ColorCodingIndexProperty);
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
                from c in Primitive.FeatureCurves.ZipIndex()
                select new { Curve = c.Value, Index = c.Index };

            var silhouetteInfos =
                from c in Primitive.SilhouetteCurves.ZipIndex(Primitive.FeatureCurves.Length)
                select new { Curve = c.Value, Index = c.Index };

            var allCurveInfos = featureInfos.Concat(silhouetteInfos);
            foreach (var curveInfo in allCurveInfos)
            {
                var strokeConverter = 
                    new DelegateConverter<int>(index => Constants.PRIMITIVE_CURVES_COLOR_CODING[index]);

                // create curve stroke
                var path = CreatePolyline(curveInfo.Curve.Points);
                path.Bind(Path.StrokeThicknessProperty, new PropertyPath(IsEmphasizedProperty), path, new DelegateConverter<bool>(
                    isEmphasized => isEmphasized ? 4.0 : 2.0));
                SetColorCodingIndex(path, curveInfo.Index);
                path.Stroke = Constants.PRIMITIVE_CURVES_COLOR_CODING[curveInfo.Index];

                grid.Children.Add(path);

                SetPrimitiveCurve(path, curveInfo.Curve);

                if (curveInfo.Curve.AssignedTo != null)
                {
                    // create perpendicular line towards the assigned curve
                    var fstPoint = curveInfo.Curve.ClosestPoint;
                    var sndPoint = fstPoint.ProjectionOnCurve(curveInfo.Curve.AssignedTo.Points).Item1;
                    var line = CreatePolyline(new Point[] { fstPoint, sndPoint });
                    line.Stroke = Constants.PRIMITIVE_CURVES_COLOR_CODING[curveInfo.Index];

                    line.Bind(Path.StrokeThicknessProperty, new PropertyPath(IsEmphasizedProperty), path, new DelegateConverter<bool>(
                        isEmphasized => isEmphasized ? 2.0 : 1.0));

                    line.Bind(Path.StrokeDashArrayProperty, new PropertyPath(IsEmphasizedProperty), path, new DelegateConverter<bool>(
                        isEmphasized => isEmphasized ? new DoubleCollection(new double[] { 2.5, 2.5 }) : new DoubleCollection(new double[] { 5, 5 })));

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
