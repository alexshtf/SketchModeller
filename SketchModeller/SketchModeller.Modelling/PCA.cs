using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Meta.Numerics.Matrices;
using Utils;

namespace SketchModeller.Modelling
{
    class PCAResult
    {
        private Vector3D[] components;
        private double[] variance;

        public PCAResult(Vector3D c1, Vector3D c2, Vector3D c3, double v1, double v2, double v3)
        {
            components = new Vector3D[3];
            variance = new double[3];

            components[0] = c1;
            components[1] = c2;
            components[2] = c3;

            variance[0] = v1;
            variance[1] = v2;
            variance[2] = v3;
        }

        public Vector3D Component(int index)
        {
            Contract.Requires(index >= 0 && index < 3);
            return components[index];
        }

        public double Variance(int index)
        {
            Contract.Requires(index >= 0 && index < 3);
            return variance[index];
        }
    }

    static class PCA3D
    {
        public static PCAResult Compute(params Point3D[] points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Length > 0);
            Contract.Ensures(Contract.Result<PCAResult>() != null);
            
            var centroid = MathUtils3D.Centroid(points);
            var vectors = new Vector3D[points.Length];
            for (int i = 0; i < vectors.Length; ++i)
                vectors[i] = points[i] - centroid;

            return Compute(vectors);
        }

        public static PCAResult Compute(params Vector3D[] vectors)
        {
            Contract.Requires(vectors != null);
            Contract.Requires(vectors.Length > 0);
            Contract.Ensures(Contract.Result<PCAResult>() != null);

            var observations = new Matrix(3, vectors.Length);
            for (int i = 0; i < vectors.Length; ++i)
            {
                observations[0, i] = vectors[i].X;
                observations[1, i] = vectors[i].Y;
                observations[2, i] = vectors[i].Z;
            }

            var covariance = observations * observations.Transpose();
            Contract.Assume(covariance.RowCount == 3);
            Contract.Assume(covariance.ColumnCount == 3);

            var covarianceSym = new SymmetricMatrix(3);
            for (int i = 0; i < 3; ++i)
                for (int j = i; j < 3; ++j)
                    covarianceSym[i, j] = covariance[i, j];

            var eig = covarianceSym.Eigensystem();
            Contract.Assume(eig.Dimension == 3);

            // Eigenvectors are ordered in ascending eigenvalue order. So we take the eigenvectors in reverse order
            return new PCAResult(
                c1: ToVector3D(eig.Eigenvector(2)),
                c2: ToVector3D(eig.Eigenvector(1)),
                c3: ToVector3D(eig.Eigenvector(0)),
                v1: eig.Eigenvalue(2),
                v2: eig.Eigenvalue(1),
                v3: eig.Eigenvalue(2));
        }

        private static Vector3D ToVector3D(ColumnVector v)
        {
            Contract.Requires(v.Dimension == 3);
            return new Vector3D(v[0], v[1], v[2]);
        }
    }
}
