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
            return CreateVisual(bgc, model =>
                {
                    model.Bind(GeometryModel3D.GeometryProperty,
                        () => bgc.BottomCenterResult,
                        () => bgc.Uresult,
                        () => bgc.Vresult,
                        () => bgc.ComponentResults,
                        (bottomCenter, U, V, components) => CreateBGCGeometry(bottomCenter, U, V, components));
                });
        }

        private static MeshGeometry3D CreateBGCGeometry(
            Point3D bottomCenter,
            Vector3D U,
            Vector3D V,
            IEnumerable<BendedCylinderComponent> components)
        {
            var Ss = (from component in components
                      select component.S).ToArray();

            var Ts = (from component in components
                      select component.T).ToArray();

            var diametersQuery = from component in components
                                 select 2 * component.Radius;

            Point3D[] path = new Point3D[Ts.Length];
            for (int i = 0; i < Ts.Length; i++)
                path[i] = bottomCenter + Ss[i] * U + Ts[i] * V;

            var builder = new MeshBuilder();
            
            builder.AddTube(
                path,
                null,
                diametersQuery.ToArray(),
                thetaDiv: 20,
                isTubeClosed: false);
            
            var geometry = builder.ToMesh(freeze: true);
            return geometry;
        }
    }
}
