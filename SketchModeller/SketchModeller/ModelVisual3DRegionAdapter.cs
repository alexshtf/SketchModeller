using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Specialized;
using System.Windows.Media.Media3D;

namespace SketchModeller
{
    class ModelVisual3DRegionAdapter : MultiItemRegionAdapterBase<ModelVisual3D, Visual3D>
    {
        public ModelVisual3DRegionAdapter(IRegionBehaviorFactory factory)
            : base(GetChildren, factory)
        {
        }

        private static IList<Visual3D> GetChildren(ModelVisual3D modelVisual3D)
        {
            return modelVisual3D.Children;
        }
    }
}
