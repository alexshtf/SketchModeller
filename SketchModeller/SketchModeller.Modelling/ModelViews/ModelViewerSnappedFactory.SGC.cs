using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Utils;
using HelixToolkit;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {
        private Visual3D CreateSgcView(SnappedStraightGenCylinder sgc)
        {
            var model = new GeometryModel3D
            {
                Material = new DiffuseMaterial { Brush = Brushes.White },
            };
            model.Bind(GeometryModel3D.GeometryProperty,
                () => sgc.AxisResult,
                () => sgc.LengthResult,
                () => sgc.BottomCenterResult,
                () => sgc.ComponentResults,
                (axis, length, bottomCenter, components) => CreateSGCGeometry(axis, length, bottomCenter, components));
            return new ModelVisual3D
            {
                Content = model
            };
        }

        private static MeshGeometry3D CreateSGCGeometry(
            Vector3D axis, 
            double length, 
            Point3D bottomCenter, 
            IEnumerable<CylinderComponent> components)
        {
            var startPoint = bottomCenter;
            var endPoint = bottomCenter + axis * length;

            var pathQuery = from component in components
                            select MathUtils3D.Lerp(startPoint, endPoint, component.Progress);

            var diametersQuery = from component in components
                                 select 2 * component.Radius;

            var builder = new MeshBuilder();
            builder.AddTube(
                pathQuery.ToArray(),
                null,
                diametersQuery.ToArray(),
                thetaDiv: 20,
                isTubeClosed: false);
            var geometry = builder.ToMesh(freeze: true);
            return geometry;
        }
    }
}
