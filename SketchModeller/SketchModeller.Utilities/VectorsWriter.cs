using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;
using System.Windows;

namespace SketchModeller.Utilities
{
    public class VectorsWriter
    {
        public readonly List<double> buffer;

        public VectorsWriter()
        {
            buffer = new List<double>();
        }

        public int Length { get { return buffer.Count; } }

        public void Write(double value)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 1);
            buffer.Add(value);
        }

        public void Write(Point3D pnt)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 3);

            Write(pnt.X);
            Write(pnt.Y);
            Write(pnt.Z);
        }

        public void Write(Vector3D vec)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 3);

            Write(vec.X);
            Write(vec.Y);
            Write(vec.Z);
        }

        public void Write(Point pnt)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 2);

            Write(pnt.X);
            Write(pnt.Y);
        }

        public void Write(Vector vec)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 2);

            Write(vec.X);
            Write(vec.Y);
        }

        public double[] ToArray()
        {
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == Length);

            return buffer.ToArray();
        }
    }
}
