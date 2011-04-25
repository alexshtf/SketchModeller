using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace Controls
{
    [ContractClass(typeof(Visual3DFactoryContract))]
    public interface IVisual3DFactory
    {
        Visual3D Create(object item);
    }

    [ContractClassFor(typeof(IVisual3DFactory))]
    abstract class Visual3DFactoryContract : IVisual3DFactory
    {
        public Visual3D Create(object item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            return null;
        }
    }

}
