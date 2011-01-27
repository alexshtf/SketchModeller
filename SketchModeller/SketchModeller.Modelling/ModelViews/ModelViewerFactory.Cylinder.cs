using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using System.Windows.Media;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerFactory
    {

        private ModelVisual3D CreateCylinderView(NewCylinder data)
        {
            var cylinder = new Cylinder();
            cylinder.Bind(Cylinder.Radius1Property, () => data.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Radius2Property, () => data.Diameter, diameter => diameter / 2);
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
