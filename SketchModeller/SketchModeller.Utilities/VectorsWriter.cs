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

        public VectorsWriter Write(double value)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 1);
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            buffer.Add(value);
            return this;
        }

        public VectorsWriter Write(Point3D pnt)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 3);
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            Write(pnt.X);
            Write(pnt.Y);
            Write(pnt.Z);
            return this;
        }

        public VectorsWriter Write(Vector3D vec)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 3);
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            Write(vec.X);
            Write(vec.Y);
            Write(vec.Z);
            return this;
        }

        public VectorsWriter Write(Point pnt)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 2);
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            Write(pnt.X);
            Write(pnt.Y);
            return this;
        }

        public VectorsWriter Write(Vector vec)
        {
            Contract.Ensures(Length == Contract.OldValue(Length) + 2);
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            Write(vec.X);
            Write(vec.Y);
            return this;
        }

        public VectorsWriter WriteRange(IEnumerable<Vector> vecs)
        {
            Contract.Requires(vecs != null);
            Contract.Ensures(Length == Contract.OldValue(Length) + 2 * vecs.Count());
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            foreach (var vec in vecs)
                Write(vec);
            return this;
        }


        public VectorsWriter WriteRange(IEnumerable<Point> pts)
        {
            Contract.Requires(pts != null);
            Contract.Ensures(Length == Contract.OldValue(Length) + 2 * pts.Count());
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            foreach (var pnt in pts)
                Write(pnt);
            return this;
        }

        public VectorsWriter WriteRange(IEnumerable<Point3D> pts)
        {
            Contract.Requires(pts != null);
            Contract.Ensures(Length == Contract.OldValue(Length) + 3 * pts.Count());
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            foreach (var pnt in pts)
                Write(pnt);
            return this;
        }

        public VectorsWriter WriteRange(IEnumerable<Vector3D> vecs)
        {
            Contract.Requires(vecs != null);
            Contract.Ensures(Length == Contract.OldValue(Length) + 3 * vecs.Count());
            Contract.Ensures(Contract.Result<VectorsWriter>() == this);

            foreach (var vec in vecs)
                Write(vec);
            return this;
        }

        public double[] ToArray()
        {
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == Length);

            return buffer.ToArray();
        }
    }
}
