using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public class VectorsReader
    {
        private readonly double[] buffer;
        private int currentPos;

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(currentPos <= buffer.Length);
        }

        public VectorsReader(double[] buffer)
        {
            Contract.Requires(buffer != null);

            this.buffer = buffer;
        }

        public int Length
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return buffer.Length;
            }
        }

        public int Position
        {
            get 
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return currentPos; 
            }
        }

        public double ReadValue()
        {
            Contract.Requires(Position <= Length - 1);
            return buffer[currentPos++];
        }

        public Point3D ReadPoint3D()
        {
            Contract.Requires(Position <= Length - 3);
            return new Point3D(ReadValue(), ReadValue(), ReadValue());
        }

        public Vector3D ReadVector3D()
        {
            Contract.Requires(Position <= Length - 3);
            return (Vector3D)ReadPoint3D();
        }

        public Point ReadPoint()
        {
            Contract.Requires(Position <= Length - 2);
            return new Point(ReadValue(), ReadValue());
        }

        public Vector ReadVector()
        {
            Contract.Requires(Position <= Length - 2);
            return (Vector)ReadPoint();
        }
    }
}
