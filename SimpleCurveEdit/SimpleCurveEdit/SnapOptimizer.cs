using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using CenterSpace.NMath.Core;
using CenterSpace.NMath.Matrix;

namespace SimpleCurveEdit
{
    public class SnapOptimizer
    {
        private readonly OptimizationPoint[] before;
        private readonly OptimizationPoint[] middle;
        private readonly OptimizationPoint[] after;
        private readonly Matrix3D proj;

        private readonly OptimizationPoint[] allPoints;
        private readonly double[] middleDistance;

        private readonly int k;
        private readonly int l;
        private readonly int m;
        private readonly int totalCount;

        public SnapOptimizer(OptimizationPoint[] before, OptimizationPoint[] middle, OptimizationPoint[] after, Matrix3D proj)
        {
            this.before = before;
            this.middle = middle;
            this.after = after;
            this.proj = proj;

            allPoints = before.Concat(middle).Concat(after).ToArray();

            k = before.Length;
            l = middle.Length;
            m = after.Length;
            totalCount = allPoints.Length;
        }

        public void Solve()
        {
            const double CLOSENESS_WEIGHT = 100;

            /**
            * We will be solving the following optimization problem:
            *	min { sum{|v_i - v_i-1|²} + sum{|v_i - v'_i|²} : Pv = y }
            * where the equation Pv = y describes projection constraints on the middle part of the curve and v
            * is a vector describing all the new positions for the points of the whole curve.
            * We can state the problem above differently:
            *	min { |Lv|² + |Fv|² - <c, v> + d : Pv = y }
            * where L is a matrix that transforms v vectors that represent the smoothness of v, c is a vector
            * describing the sum of inner products of v_i with v'_i and F is a matrix that filters out the middle
            * part of v (leaves only before and after).
            * Using Lagrange multipliers we can transform the problem into a linear system of equations:
            *	( L'L + F'F    -P' ) (v)   (c)
            *   (                  ) ( ) = ( )
            *   ( P             0  ) (u)   (y)
            * where u is the Lagrange multiplier associated with the constraint Pv = y. 
            * The following code deals with creating the above matrices and solving this linear system.
            */

            // we will describe hard projection constraints in matrix form Pv = y (v is the vector of ALL points)
            DoubleVector y;
            DoubleMatrix P;
            BuildProjectionConstraints(out y, out P);

            // we will describe the smoothness term as |Lv|² where L is the matrix
            // transforming v into vectors that represent the smoothness of v.
            var L = BuildSmoothMatrix();

            // we will build now the filter matrix that filters out everything except 
            // the middle elements.
            var F = BuildFilterMatrix();

            // build distance regulazigin matrix so that length of the constrained part
            // of the curve don't grow too much.
            var D = BuildDistanceRegularizingMatrix();

            var c = new DoubleVector(3 * totalCount);
		    for(int i = 0; i < k; ++i)
		    {
			    c[3 * i + 0] = before[i].Original.X;
			    c[3 * i + 1] = before[i].Original.Y;
			    c[3 * i + 2] = before[i].Original.Z;
		    }
		    for(int i = 0; i < m; ++i)
		    {
			    c[3 * k + 3 * l + 3 * i + 0] = after[i].Original.X;
                c[3 * k + 3 * l + 3 * i + 1] = after[i].Original.Y;
                c[3 * k + 3 * l + 3 * i + 2] = after[i].Original.Z;
		    }
            c = CLOSENESS_WEIGHT * c;

            // now we build the big matrix
            int totalMatrixSize = 3 * totalCount + 2 * l;
            var M = new DoubleMatrix(totalMatrixSize, totalMatrixSize);
            var mpt = -P.Transpose();
            var objectiveMatrices = 
                NMathFunctions.TransposeProduct(D, D) + 
                NMathFunctions.TransposeProduct(L, L) + 
                CLOSENESS_WEIGHT * NMathFunctions.TransposeProduct(F, F);

            var topLeft = M[new Slice(0, 3 * totalCount), new Slice(0, 3 * totalCount)];
            var topRight = M[new Slice(0, 3 * totalCount), new Slice(3 * totalCount, 2 * l)];
            var botLeft = M[new Slice(3 * totalCount, 2 * l), new Slice(0, 3 * totalCount)];

            Copy(source: objectiveMatrices, target: topLeft);
            Copy(source: mpt, target: topRight);
            Copy(source: P, target: botLeft);

            // and the big known vector
            var bItems = c.ToArray().Concat(y.ToArray());
            var b = new DoubleVector(bItems.ToArray());

            var luSolver = new DoubleLUFact(M);
            var results = luSolver.Solve(b);

            for (int i = 0; i < allPoints.Length; ++i)
            {
                var px = results[3 * i];
                var py = results[3 * i + 1];
                var pz = results[3 * i + 2];
                allPoints[i].New = new Point3D(px, py, pz);
            }
        }

        private DoubleMatrix BuildFilterMatrix()
        {
            var F = new DoubleMatrix(3 * k + 3 * m, 3 * totalCount);
            Action<int, int, int> fillIdentify = (baseRow, baseCol, count) =>
                {
                    for (int i = 0; i < count; ++i)
                        F[baseRow + i, baseCol + i] = 1;
                };
            fillIdentify(0, 0, 3 * k);
            fillIdentify(3 * k, 3 * k + 3 * l, 3 * m);
            return F;
        }

        private DoubleMatrix BuildSmoothMatrix()
        {
            var L = new DoubleMatrix(3 * (totalCount - 2), 3 * totalCount); // Laplacian
            for (int i = 0; i < totalCount - 2; ++i)
            {
                int row = 3 * i;
                int col = 3 * i;

                L[row + 0, col + 0] = -0.5;
                L[row + 1, col + 1] = -0.5;
                L[row + 2, col + 2] = -0.5;

                L[row + 0, col + 3] = 1;
                L[row + 1, col + 4] = 1;
                L[row + 2, col + 5] = 1;

                L[row + 0, col + 6] = -0.5;
                L[row + 1, col + 7] = -0.5;
                L[row + 2, col + 8] = -0.5;
            }
            return L;

        }

        private DoubleMatrix BuildDistanceRegularizingMatrix()
        {
            // sum of diff for middle, 1 of before and 1 of after
            var D = new DoubleMatrix(3 * (l + 1), 3 * totalCount);
            for (int i = 0; i < l + 1; ++i)
            {
                var row = 3 * i;
                var col = 3 * k + 3 * i - 3;

                D[row + 0, col + 0] = -1;
                D[row + 1, col + 1] = -1;
                D[row + 2, col + 2] = -1;
                D[row + 0, col + 3] = 1;
                D[row + 1, col + 4] = 1;
                D[row + 2, col + 5] = 1;
            }
            return D;
        }

        private void BuildProjectionConstraints(out DoubleVector y, out DoubleMatrix P)
        {
            y = new DoubleVector(2 * l);
            P = new DoubleMatrix(2 * l, 3 * totalCount);

            var p11 = proj.M11; var p12 = proj.M12; var p13 = proj.M13; var p14 = proj.M14;
            var p21 = proj.M21; var p22 = proj.M22; var p23 = proj.M23; var p24 = proj.M24;
            var p31 = proj.M31; var p32 = proj.M32; var p33 = proj.M33; var p34 = proj.M34;
            var p44 = proj.M44;
            var p41 = proj.OffsetX;
            var p42 = proj.OffsetY;
            var p43 = proj.OffsetZ;
            for (int i = 0; i < l; ++i)
            {
                var u = middle[i].ProjConstraint.X;
                var v = middle[i].ProjConstraint.Y;

                var row = 2 * i;
                var col = 3 * k + 3 * i;

                y[row + 0] = u * p44 - p41;
                y[row + 1] = v * p44 - p42;

                P[row + 0, col + 0] = p11 - u * p14;
                P[row + 0, col + 1] = p21 - u * p24;
                P[row + 0, col + 2] = p31 - u * p34;
                P[row + 1, col + 0] = p12 - v * p14;
                P[row + 1, col + 1] = p22 - v * p24;
                P[row + 1, col + 2] = p32 - v * p34;
            }
        }

        private static void Copy(DoubleMatrix source, DoubleMatrix target)
        {
            for (int row = 0; row < target.Rows; ++row)
                for (int col = 0; col < target.Cols; ++col)
                    target[row, col] = source[row, col];
        }
    }
}
