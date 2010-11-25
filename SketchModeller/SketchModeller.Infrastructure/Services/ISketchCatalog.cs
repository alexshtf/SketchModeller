using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Services
{
    [ContractClass(typeof(SketchCatalogContract))]
    public interface ISketchCatalog
    {
        IObservable<string[]> GetSketchNamesAsync();
        IObservable<SketchData> LoadSketchAsync(string sketchName);
    }

    [ContractClassFor(typeof(ISketchCatalog))]
    public abstract class SketchCatalogContract : ISketchCatalog
    {
        public IObservable<string[]> GetSketchNamesAsync()
        {
            Contract.Ensures(Contract.Result<IObservable<string[]>>() != null);
            return null;
        }

        public IObservable<SketchData> LoadSketchAsync(string sketchName)
        {
            Contract.Requires(!string.IsNullOrEmpty(sketchName));
            Contract.Ensures(Contract.Result<IObservable<SketchData>>() != null);
            return null;
        }
    }

}
