using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Petzold.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="ModelVisualBase"/>.
    /// </summary>
    public static class ModelVisualBaseHelper
    {
        /// <summary>
        /// Sets front and back materials
        /// </summary>
        /// <param name="mvb">The ModelVisualBase object.</param>
        /// <param name="front">The front material</param>
        /// <param name="back">The back material</param>
        public static void SetMaterials(this ModelVisualBase mvb, Material front, Material back)
        {
            Contract.Requires(mvb != null);
            Contract.Ensures(mvb.Material == front);
            Contract.Ensures(mvb.BackMaterial == back);

            mvb.Material = front;
            mvb.BackMaterial = back;
        }

        /// <summary>
        /// Sets the front and back materials
        /// </summary>
        /// <param name="mvb">The ModelVisualBase object.</param>
        /// <param name="frontBack">A tuple containing front and back materials</param>
        public static void SetMaterials(this ModelVisualBase mvb, Tuple<Material, Material> frontBack)
        {
            Contract.Requires(mvb != null);
            Contract.Ensures(frontBack == null || mvb.Material == frontBack.Item1);
            Contract.Ensures(frontBack == null || mvb.BackMaterial == frontBack.Item2);
            Contract.Ensures(frontBack != null || mvb.Material == null);
            Contract.Ensures(frontBack != null || mvb.BackMaterial == null);

            if (frontBack == null)
                mvb.Material = mvb.BackMaterial = null;
            else
                mvb.SetMaterials(frontBack.Item1, frontBack.Item2);
        }
    }
}
