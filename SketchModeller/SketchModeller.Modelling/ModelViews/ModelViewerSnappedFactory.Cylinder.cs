using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Diagnostics.Contracts;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {
        private Visual3D CreateCylinderView(SketchModeller.Infrastructure.Data.SnappedCylinder cylinderData)
        {
            if (cylinderData.TopCircle == null ||
                cylinderData.BottomCircle == null)
            {
                return new ModelVisual3D();
            }
            else
            {
                Contract.Assume(cylinderData.TopCircle.Length == cylinderData.BottomCircle.Length);
                var m = cylinderData.TopCircle.Length;

                var topPoints = cylinderData.TopCircle;
                var botPoints = cylinderData.BottomCircle;

                // top points indices [0 .. m-1]
                var topIdx = System.Linq.Enumerable.Range(0, m).ToArray();

                // bottom points indices [m .. 2*m - 1]
                var bottomIdx = System.Linq.Enumerable.Range(m, m).ToArray();
                Contract.Assume(topIdx.Length == bottomIdx.Length);

                // create cylinder geometry
                var geometry = new MeshGeometry3D();
                geometry.Positions = new Point3DCollection(topPoints.Concat(botPoints));
                geometry.TriangleIndices = new Int32Collection();
                for (int i = 0; i < m; ++i)
                {
                    var j = (i + 1) % m;
                    var pc = topIdx[i];
                    var pn = topIdx[j];
                    var qc = bottomIdx[i];
                    var qn = bottomIdx[j];

                    geometry.TriangleIndices.AddMany(pc, qc, pn);
                    geometry.TriangleIndices.AddMany(qc, qn, pn);
                }

                return CreateVisual(geometry);
            }
        }
    }
}
