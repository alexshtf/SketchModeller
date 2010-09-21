using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Utils;

namespace MultiviewCurvesToCyl
{
    enum HandleKind
    {
        Start,
        End,
    }

    /// <summary>
    /// Interaction logic for SketchCurveView.xaml
    /// </summary>
    public partial class SketchCurveView : UserControl
    {
        public SketchCurveView()
        {
            InitializeComponent();
        }

        private SketchCurveViewModel ViewModel
        {
            get { return DataContext as SketchCurveViewModel; } // we assume that the data-context is our view model
        }

        private void OnStartDragDelta(object sender, DragDeltaEventArgs e)
        {
            CastUtils.MatchClass<Thumb>(sender, thumb =>
                {
                    var pointIndex = PointIndexFromThumbDrag(e, thumb);
                    ViewModel.StartIndex = pointIndex;
                });
        }

        private void OnEndDragDelta(object sender, DragDeltaEventArgs e)
        {
            CastUtils.MatchClass<Thumb>(sender, thumb =>
                {
                    var pointIndex = PointIndexFromThumbDrag(e, thumb);
                    ViewModel.EndIndex = pointIndex;
                });
        }

        private int PointIndexFromThumbDrag(DragDeltaEventArgs e, Thumb thumb)
        {
            var left = Canvas.GetLeft(thumb);
            var top = Canvas.GetTop(thumb);

            var newPos = new Point
            {
                X = left + e.HorizontalChange,
                Y = top + e.VerticalChange,
            };

            var proj = newPos.ProjectionOnCurve(ViewModel.Curve.PolylinePoints);
            var pnt = proj.Item1;
            var segmentIndex = proj.Item3;

            var p1 = ViewModel.Curve.PolylinePoints[segmentIndex];
            var p2 = ViewModel.Curve.PolylinePoints[segmentIndex + 1];

            int newIndex = (p1 - pnt).Length < (p2 - pnt).Length ? segmentIndex : segmentIndex + 1;
            return newIndex;
        }
    }
}
