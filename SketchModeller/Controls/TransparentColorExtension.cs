using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;

namespace Controls
{
    [MarkupExtensionReturnType(typeof(Color))]
    public class TransparentColorExtension : MarkupExtension
    {
        public Color BaseColor { get; set; }
        public byte Alpha { get; set; }

        public TransparentColorExtension()
        {
            Alpha = 255;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var result = Color.FromArgb(Alpha, BaseColor.R, BaseColor.G, BaseColor.B);
            return result;
        }
    }
}
