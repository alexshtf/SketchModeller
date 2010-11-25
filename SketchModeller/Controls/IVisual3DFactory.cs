using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Controls
{
    public interface IVisual3DFactory
    {
        Visual3D Create(object item);
    }
}
