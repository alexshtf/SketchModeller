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
using System.Windows.Media.Media3D;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Utils;

namespace MultiviewCurvesToCyl
{
    #region CylinderOrientationToDegreesConverter class

    [ValueConversion(typeof(Vector3D), typeof(double))]
    class CylinderOrientationToDegreesConverter : IValueConverter
    {
        public static readonly CylinderOrientationToDegreesConverter Instance =
            new CylinderOrientationToDegreesConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Vector3D)
            {
                var concrete = (Vector3D)value;
                return Vector3D.AngleBetween(concrete, MathUtils3D.UnitX);
            }
            else
                return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way converters don't support ConvertBack.");
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for NewCylinderView2D.xaml
    /// </summary>
    public partial class NewCylinderView2D : UserControl
    {
        private const double MIN_LENGTH = 5;
        private const double MIN_RADIUS = 5;

        public NewCylinderView2D()
        {
            InitializeComponent();
        }

        /// <summary>
        /// We assume that the data context is the view-model.
        /// </summary>
        private NewCylinderViewModel ViewModel
        {
            get { return (NewCylinderViewModel)DataContext; }
        }

        private void MoveHandleDragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.Center = ViewModel.Center + AsVector(e);
        }

        private void TopDragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.Radius = Math.Max(MIN_RADIUS, ViewModel.Radius - e.VerticalChange / 4);
        }

        private void BottomDragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.Radius = Math.Max(MIN_RADIUS, ViewModel.Radius + e.VerticalChange / 4);
        }

        private void LeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.Length = Math.Max(MIN_LENGTH, ViewModel.Length - e.HorizontalChange / 2);
        }

        private void RightDragDelta(object sender, DragDeltaEventArgs e)
        {
            ViewModel.Length = Math.Max(MIN_LENGTH, ViewModel.Length + e.HorizontalChange / 2);
        }

        private void RotateDragDelta(object sender, DragDeltaEventArgs e)
        {
            sender.MatchClass<FrameworkElement>(handle =>
            {
                var origin = default(Point);
                var handlePos = handle.TranslatePoint(new Point(), container);
                var newPos = handlePos + AsVector(e).IgnoreZ();

                var rotationDegrees = Vector.AngleBetween(handlePos - origin, newPos - origin);
                var orientationDegrees = Vector3D.AngleBetween(MathUtils3D.UnitX, ViewModel.Orientation);
                var newOrientationDegrees = orientationDegrees + rotationDegrees;

                var newOrientationRadians = newOrientationDegrees * Math.PI / 180.0;
                ViewModel.Orientation = new Vector3D(Math.Cos(newOrientationRadians), Math.Sin(newOrientationRadians), 0);
            });
        }

        private static Vector3D AsVector(DragDeltaEventArgs e)
        {
            return new Vector3D(e.HorizontalChange, e.VerticalChange, 0);
        }

        private static Vector AsVector(Point p)
        {
            return p - default(Point);
        }

    }
}
