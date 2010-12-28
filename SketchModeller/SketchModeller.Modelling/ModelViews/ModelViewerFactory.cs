using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using System.Windows.Media;

namespace SketchModeller.Modelling.ModelViews
{
    class ModelViewerFactory : IVisual3DFactory
    {
        public static readonly ModelViewerFactory Instance = new ModelViewerFactory();

        public Visual3D Create(object item)
        {
            var result = new ModelVisual3D();
            item.MatchClass<NewCylinder>(cylinderData => result = CreateCylinderView(cylinderData));

            return result;
        }

        private ModelVisual3D CreateCylinderView(NewCylinder data)
        {
            var cylinder = new Cylinder();
            cylinder.Bind(Cylinder.Radius1Property, () => data.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Radius2Property, () => data.Diameter, diameter => diameter / 2);
            cylinder.Bind(Cylinder.Point1Property,
                () => data.Center, 
                () => data.Axis, 
                () => data.Length,
                (center, axis, length) => center.ToWpfPoint() + 0.5 * length * axis.ToWpfVector().Normalized());
            cylinder.Bind(Cylinder.Point2Property,
                () => data.Center, 
                () => data.Axis, 
                () => data.Length,
                (center, axis, length) => center.ToWpfPoint() - 0.5 * length * axis.ToWpfVector().Normalized());

            cylinder.Material = new DiffuseMaterial { Brush = Brushes.White };

            return cylinder;
        }
    }
}
