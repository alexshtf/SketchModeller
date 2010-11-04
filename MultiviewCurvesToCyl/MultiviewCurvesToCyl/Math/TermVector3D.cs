using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Windows.Media.Media3D;

namespace MultiviewCurvesToCyl
{
    class TermVector3D
    {
        private Term lengthSquared;

        public TermVector3D(Term x, Term y, Term z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Term X { get; private set; }
        public Term Y { get; private set; }
        public Term Z { get; private set; }

        public Term LengthSquared
        {
            get 
            { 
                if (lengthSquared == null)
                    lengthSquared = TermBuilder.Sum(
                        TermBuilder.Power(X, 2), 
                        TermBuilder.Power(Y, 2), 
                        TermBuilder.Power(Z, 2));
                return lengthSquared;
            }
        }

        public static Term operator*(TermVector3D left, TermVector3D right)
        {
            return TermBuilder.Sum(
                left.X * right.X,
                left.Y * right.Y,
                left.Z * right.Z);
        }

        public static TermVector3D CrossProduct(TermVector3D left, TermVector3D right)
        {
            return new TermVector3D(
                    (left.Y * right.Z) - (left.Z * right.Y),
                    (left.Z * right.X) - (left.X * right.Z),
                    (left.X * right.Y) - (left.Y * right.X)
                );
        }

        public static TermVector3D operator+(TermVector3D left, TermVector3D right)
        {
            return new TermVector3D(
                left.X + right.X, 
                left.Y + right.Y, 
                left.Z + right.Z);
        }

        public static TermVector3D operator-(TermVector3D left, TermVector3D right)
        {
            return new TermVector3D(
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z);
        }

        public static TermVector3D operator-(TermVector3D operand)
        {
            return new TermVector3D(
                0 - operand.X,
                0 - operand.Y,
                0 - operand.Z);
        }

        public static TermVector3D operator*(Term scalar, TermVector3D vector)
        {
            return new TermVector3D(
                scalar * vector.X,
                scalar * vector.Y,
                scalar * vector.Z);
        }

        public static TermVector3D operator*(TermVector3D vector, Term scalar)
        {
            return scalar * vector;
        }

        public static TermVector3D operator/(TermVector3D vector, double scalar)
        {
            return vector * (1 / scalar);
        }

        public static implicit operator TermVector3D(Vector3D input)
        {
            return new TermVector3D(input.X, input.Y, input.Z);
        }

        public static implicit operator TermVector3D(Point3D input)
        {
            return new TermVector3D(input.X, input.Z, input.Z);
        }
    }
}
