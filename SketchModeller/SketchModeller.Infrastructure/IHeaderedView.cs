using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure
{
    public interface IHeaderedView
    {
        string ViewName { get; }
    }
}
