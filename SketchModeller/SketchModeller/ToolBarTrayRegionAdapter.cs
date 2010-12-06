using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Specialized;

namespace SketchModeller
{
    class ToolBarTrayRegionAdapter : MultiItemRegionAdapterBase<ToolBarTray, ToolBar>
    {
        public ToolBarTrayRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(GetToolBars, regionBehaviorFactory)
        {
        }
        
        private static IList<ToolBar> GetToolBars(ToolBarTray tray)
        {
            return tray.ToolBars;
        }
    }
}
