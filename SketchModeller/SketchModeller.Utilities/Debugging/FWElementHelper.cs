using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Utilities.Debugging
{
    static class FWElementHelper
    {
        public static void FakeLayout(FrameworkElement fwElement)
        {
            fwElement.Measure(new Size(fwElement.Width, fwElement.Height));
            fwElement.Arrange(new Rect(0, 0, fwElement.DesiredSize.Width, fwElement.DesiredSize.Height));
        }
    }
}
