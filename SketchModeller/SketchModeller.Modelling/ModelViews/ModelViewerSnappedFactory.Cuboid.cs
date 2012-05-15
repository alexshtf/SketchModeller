using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Utils;
using HelixToolkit;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {

        public static Visual3D CreateCuboidView(SnappedCuboid cuboidData)
        {
            Contract.Requires(cuboidData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);
            var visual = new ModelVisual3D();

            var meshBuilder = new MeshBuilder(true, true);
            //meshBuilder.AddBox(cuboidData.CenterResult, cuboidData.WidthResult, cuboidData.HeightResult, cuboidData.DepthResult);
            //meshBuilder.AddBox(new Point3D(0,0,0), cuboidData.WidthResult, cuboidData.HeightResult, cuboidData.DepthResult);

            meshBuilder.AddArrow(cuboidData.Origin, cuboidData.Origin + 0.5 * cuboidData.Wresult, 0.05);
            var geometry = meshBuilder.ToMesh();
            visual.Children.Add(CreateVisual(geometry, Brushes.Green));

            meshBuilder = new MeshBuilder(true, true);
            meshBuilder.AddArrow(cuboidData.Origin, cuboidData.Origin + 0.5 * cuboidData.Hresult, 0.05);
            geometry = meshBuilder.ToMesh();
            visual.Children.Add(CreateVisual(geometry, Brushes.Red));

            meshBuilder = new MeshBuilder(true, true);
            meshBuilder.AddArrow(cuboidData.Origin, cuboidData.Origin + 0.5 * cuboidData.Dresult, 0.05);
            geometry = meshBuilder.ToMesh();
            visual.Children.Add(CreateVisual(geometry, Brushes.Blue));

            //meshBuilder = new MeshBuilder(true, true);
            //Point3D[] points = geometry.Positions.ToArray();
            //TransformPoints(points, cuboidData.CenterResult,  -cuboidData.Hresult, -cuboidData.Wresult, cuboidData.Dresult);
            //geometry.Positions = new Point3DCollection(points); 
            visual.Children.Add(CreateCuboidView(cuboidData.CenterResult, cuboidData.WidthResult, cuboidData.HeightResult, cuboidData.DepthResult,
                                    cuboidData.Wresult, cuboidData.Hresult, cuboidData.Dresult, cuboidData));
            //return CreateVisual(geometry, cuboidData, false);
            return visual;
        }
    }
}
