using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using Utils;
using System.Windows.Media;

namespace SketchModeller.Modelling.ModelViews
{
    public partial class ModelViewerFactory
    {
        private Visual3D CreateConeView(NewCone data)
        {
            var cylinder = new Cylinder();
            cylinder.Bind(Cylinder.Radius1Property, () => data.TopRadius);
            cylinder.Bind(Cylinder.Radius2Property, () => data.BottomRadius);
            cylinder.Bind(Cylinder.Point1Property,
                () => data.Center,
                () => data.Axis,
                () => data.Length,
                (center, axis, length) => center + 0.5 * length * axis.Normalized());
            cylinder.Bind(Cylinder.Point2Property,
                () => data.Center,
                () => data.Axis,
                () => data.Length,
                (center, axis, length) => center - 0.5 * length * axis.Normalized());

            cylinder.Material = new DiffuseMaterial { Brush = Brushes.White };

            return cylinder;
        }
    }
}
