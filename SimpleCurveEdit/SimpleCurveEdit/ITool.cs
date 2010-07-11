using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SimpleCurveEdit
{
    interface ITool
    {
        void MouseDown(Point position);
        void MouseMove(Point position);
        void MouseUp(Point position);
    }
}
