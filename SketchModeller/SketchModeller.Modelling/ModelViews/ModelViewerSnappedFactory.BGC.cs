using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Utils;
using HelixToolkit;
using System.Windows;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {
        private Visual3D CreateBgcView(SnappedBendedGenCylinder bgc)
        {
            //MessageBox.Show("Snapped Factory");
            var model = new GeometryModel3D
            {
                Material = new DiffuseMaterial { Brush = Brushes.White },
            };
            if (bgc.pntseq != null)
            model.Bind(GeometryModel3D.GeometryProperty,
                () => bgc.AxisResult,
                () => bgc.LengthResult,
                () => bgc.BottomCenterResult,
                () => bgc.ComponentResults,
                (axis, length, bottomCenter, components) => CreateBGCGeometry(axis, length, bottomCenter, components,bgc.pntseq));
            return new ModelVisual3D
            {
                Content = model
            };
        }

        private static MeshGeometry3D CreateBGCGeometry(
            Vector3D axis,
            double length,
            Point3D bottomCenter,
            IEnumerable<BendedCylinderComponent> components,
            Point[] botEllipse)
        {
            //MessageBox.Show("Snapped Factory");
            var startPoint = bottomCenter;
            var endPoint = bottomCenter + axis * length;

            var pathQuery = from component in components
                            select component.Pnt3D;

            /*var pathQuery2D = from component in components
                              select new Point3D(component.Pnt2D.X, component.Pnt2D.Y, 0.0);*/

            //var path = pathQuery.Reverse();

            var diametersQuery = from component in components
                                 select 2 * component.Radius;

            var builder = new MeshBuilder();
            /*builder.AddTube(
                pathQuery.ToArray(),
                null,
                diametersQuery.ToArray(),
                thetaDiv: 20,
                isTubeClosed: false);*/
            foreach (Point pnt in botEllipse)
            {
                builder.AddSphere(new Point3D(pnt.X, pnt.Y, 0.0), 0.01);
            }
            //MessageBox.Show(String.Format("{0},{1},{2}", axis.X, axis.Y, axis.Z));
            //MessageBox.Show(String.Format("Length={0}",length));
            var comp = components.ToArray();
            //foreach (var comp in components)
            builder.AddSphere(new Point3D(comp[0].Pnt2D.X, comp[0].Pnt2D.Y, 0.0), 0.02);
            var StartArrow = components.First().Pnt3D;
            var EndArrow = StartArrow + 3 * axis;
            builder.AddArrow(StartArrow, EndArrow, 0.05);
            var geometry = builder.ToMesh(freeze: true);
            return geometry;
        }
    }
}
