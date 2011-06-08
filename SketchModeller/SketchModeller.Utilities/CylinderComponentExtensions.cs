using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Extension/static methods for the <see cref="CylinderComponent"/> class
    /// </summary>
    public static class CylinderComponentExtensions
    {
        [Pure]
        public static CylinderComponent[] CloneArray(this CylinderComponent[] components)
        {
            // components != null ==> result != null
            Contract.Ensures(components == null || Contract.Result<CylinderComponent[]>() != null);

            // components == null ==> result == null
            Contract.Ensures(components != null || Contract.Result<CylinderComponent[]>() == null);

            // components != null ==> result is really a clone of components
            Contract.Ensures(components == null || Contract.ForAll(0, components.Length, i =>
                !object.ReferenceEquals(components[i], Contract.Result<CylinderComponent[]>()[i]) &&
                components[i].Radius == Contract.Result<CylinderComponent[]>()[i].Radius &&
                components[i].Progress == Contract.Result<CylinderComponent[]>()[i].Progress));


            if (components == null)
                return null;

            var resultQuery =
                from component in components
                select new CylinderComponent(component.Radius, component.Progress);

            return resultQuery.ToArray();
        }
    }
}
