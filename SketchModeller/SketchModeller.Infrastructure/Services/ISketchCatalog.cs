using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Services
{
    /// <summary>
    /// A service through which components load the existing sketches in the system.
    /// </summary>
    [ContractClass(typeof(SketchCatalogContract))]
    public interface ISketchCatalog
    {
        /// <summary>
        /// Asynchronously gets all the sketch names that exist in the system.
        /// </summary>
        /// <returns>An observable that provides the sketch names when the operation completes.</returns>
        IObservable<string[]> GetSketchNamesAsync();

        /// <summary>
        /// Asynchronously loads a sketch given its name.
        /// </summary>
        /// <param name="sketchName">The name of the sketch to load.</param>
        /// <returns>An observable that provides the sketch names when the operation completes.</returns>
        IObservable<SketchData> LoadSketchAsync(string sketchName);

        /// <summary>
        /// Asynchronously saves a sketch given its name and data.
        /// </summary>
        /// <param name="sketchName">The name of the sketch</param>
        /// <param name="sketchData">The sketch data</param>
        /// <returns>An observable, that notifies that the operation completes.</returns>
        IObservable<Unit> SaveSketchAsync(string sketchName, SketchData sketchData);
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

        public IObservable<Unit> SaveSketchAsync(string sketchName, SketchData sketchData)
        {
            Contract.Requires(!string.IsNullOrEmpty(sketchName));
            Contract.Requires(sketchData != null);
            Contract.Ensures(Contract.Result<IObservable<Unit>>() != null);
            return null;
        }
    }

}
