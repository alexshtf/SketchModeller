using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class NewPrimitive : SelectablePrimitive
    {
    }
}
