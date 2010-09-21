using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace MultiviewCurvesToCyl
{
    interface IModelFactory
    {
        Model3D Create(object dataItem);
    }
}
