using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using SketchModeller.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.Snap
{
    public class PathBrushConverter : IMultiValueConverter
    {
        public static readonly Brush NORMAL_BRUSH = Brushes.Black;
        public static readonly Brush SELECTED_BRUSH = Brushes.Blue;

        private readonly Random random = new Random();
        private Dictionary<Snapper.CurveCategory, Brush> categoryBrushes = new Dictionary<Snapper.CurveCategory, Brush>();

        public void UpdateCategories(Dictionary<Snapper.CurveCategory, Brush> newCategoryBrushes)
        {
            categoryBrushes = newCategoryBrushes;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            #region validations

            if (values.Length != 2)
            {
                Trace.TraceError("Number of inputs is invalid");
                return Binding.DoNothing;
            }

            if (!(values[0] is bool))
            {
                Trace.TraceError("First value must be a boolean");
                return Binding.DoNothing;
            }

            if (values[1] != null && !(values[1] is Snapper.CurveCategory))
            {
                Trace.TraceError("Second value must be null or a category");
                return Binding.DoNothing;
            }

            #endregion

            var isSelected = (bool)values[0];
            var category = (Snapper.CurveCategory)values[1];

            if (isSelected)
                return SELECTED_BRUSH;

            if (category == null)
                return NORMAL_BRUSH;

            return categoryBrushes[category];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }
    }
}
