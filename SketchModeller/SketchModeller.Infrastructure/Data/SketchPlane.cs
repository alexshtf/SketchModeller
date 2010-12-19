using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;

using WpfPoint3D = System.Windows.Media.Media3D.Point3D;

namespace SketchModeller.Infrastructure.Data
{
    public class SketchPlane
    {
        public static readonly SketchPlane Default = 
            new SketchPlane(
                name: "Default",
                center: MathUtils3D.Origin,
                xAxis: MathUtils3D.UnitX,
                yAxis: MathUtils3D.UnitY,
                normal: MathUtils3D.UnitZ,
                width: 2,
                height: 2);

        public SketchPlane(string name, WpfPoint3D center, Vector3D xAxis, Vector3D yAxis, Vector3D normal, double width, double height)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(MathUtils3D.AreOrthogonal(xAxis, yAxis));
            Contract.Requires(MathUtils3D.AreOrthogonal(xAxis, normal));
            Contract.Requires(MathUtils3D.AreOrthogonal(yAxis, normal));

            xAxis.Normalize();
            yAxis.Normalize();
            normal.Normalize();
            
            Name = name;
            Center = center;
            XAxis = xAxis;
            YAxis = yAxis;
            Normal = normal;
            Width = width;
            Height = height;
        }

        public string Name { get; private set; }
        public WpfPoint3D Center { get; private set; }
        public Vector3D XAxis { get; private set; }
        public Vector3D YAxis { get; private set; }
        public Vector3D Normal { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
    }
}
