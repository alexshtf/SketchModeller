using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Extension/static methods for the <see cref="BendedCylinderComponent"/> class
    /// </summary>
    public static class BendedCylinderComponentExtensions
    {
        [Pure]
        public static BendedCylinderComponent[] CloneArray(this BendedCylinderComponent[] components)
        {
            // components != null ==> result != null
            Contract.Ensures(components == null || Contract.Result<BendedCylinderComponent[]>() != null);

            // components == null ==> result == null
            Contract.Ensures(components != null || Contract.Result<BendedCylinderComponent[]>() == null);

            // components != null ==> result is really a clone of components
            Contract.Ensures(components == null || Contract.ForAll(0, components.Length, i =>
                !object.ReferenceEquals(components[i], Contract.Result<BendedCylinderComponent[]>()[i]) &&
                components[i].Radius == Contract.Result<BendedCylinderComponent[]>()[i].Radius &&
                components[i].Progress == Contract.Result<BendedCylinderComponent[]>()[i].Progress));


            if (components == null)
                return null;

            var resultQuery =
                from component in components
                select new BendedCylinderComponent(component.Radius, component.Progress, component.Pnt3D, component.Pnt2D);

            return resultQuery.ToArray();
        }
    }
}
