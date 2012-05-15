using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;
using SketchModeller.Modelling.Computations;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;

using Enumerable = System.Linq.Enumerable;
using TermUtils = SketchModeller.Utilities.TermUtils;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.Snap
{
    class CuboidSnapper : BasePrimitivesSnapper<NewCuboid, SnappedCuboid>
    {
        protected override SnappedCuboid Create(NewCuboid newPrimitive)
        {
            var snappedPrimitive = InitNewSnapped(newPrimitive);
            snappedPrimitive.SnappedTo =
                newPrimitive.AllCurves
                .Select(c => c.AssignedTo)
                .Where(c => c != null)
                .ToArray();

            snappedPrimitive.CubicCorner = newPrimitive.ArrayOfCorners[newPrimitive.ActiveCubicCorner];
            snappedPrimitive.CubicCornerIdx = newPrimitive.ActiveCubicCorner;
            /*snappedPrimitive.HeightSnappedTo = newPrimitive.LUFcubicCorner[1].AssignedTo;
            snappedPrimitive.DepthSnappedTo = newPrimitive.LUFcubicCorner[2].AssignedTo;*/

            return snappedPrimitive;
        }

        private SnappedCuboid InitNewSnapped(NewCuboid newPrimitive)
        {
            var result = new SnappedCuboid
            {
                Center = SnapperHelper.GenerateVarVector(),
                Width = new Variable(),
                Height = new Variable(),
                Depth = new Variable(),
                Wv = SnapperHelper.GenerateVarVector(),
                Hv = SnapperHelper.GenerateVarVector(),
                Dv = SnapperHelper.GenerateVarVector(),

                CenterResult = newPrimitive.Center,
                WidthResult = newPrimitive.Width,
                HeightResult = newPrimitive.Height,
                DepthResult = newPrimitive.Depth,
                Wresult = newPrimitive.W,
                Hresult = newPrimitive.H,
                Dresult = newPrimitive.D,
            };
            return result;
        }

        protected override Tuple<Term, Term[]> Reconstruct(SnappedCuboid snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            Point O = NewPrimitiveExtensions.FindCoincidentPoint(snappedPrimitive.CubicCorner);
            Point A = NewPrimitiveExtensions.FindEndPoint(snappedPrimitive.CubicCorner[0].AssignedTo, O);
            Point B = NewPrimitiveExtensions.FindEndPoint(snappedPrimitive.CubicCorner[1].AssignedTo, O);
            Point C = NewPrimitiveExtensions.FindEndPoint(snappedPrimitive.CubicCorner[2].AssignedTo, O);
            Point3D Op = NewPrimitiveExtensions.FindCoincident3DPoint(snappedPrimitive.CubicCorner);
            EnhancedPrimitiveCurve ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[0];
            Point3D Ap = NewPrimitiveExtensions.FindEnd3DPoint(ec.Points3D, Op);
            ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[1];
            Point3D Bp = NewPrimitiveExtensions.FindEnd3DPoint(ec.Points3D, Op);
            ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[2];
            Point3D Cp = NewPrimitiveExtensions.FindEnd3DPoint(ec.Points3D, Op);
            
            O.Y = -O.Y;
            A.Y = -A.Y;
            B.Y = -B.Y;
            C.Y = -C.Y;
            Vector OA = A - O;
            Vector OB = B - O;
            Vector OC = C - O;
            Debug.WriteLine("O=({0},{1})", O.X, O.Y);
            Debug.WriteLine("A=({0},{1})", A.X, A.Y);
            Debug.WriteLine("B=({0},{1})", B.X, B.Y);
            Debug.WriteLine("C=({0},{1})", C.X, C.Y);
            
            Vector OAn = OA.Normalized();
            Vector OBn = OB.Normalized();
            Vector OCn = OC.Normalized();

            double dotABn = OAn * OBn;
            double dotACn = OAn * OCn;
            double dotBCn = OBn * OCn;

            double a = Math.Acos(dotABn);
            double b = Math.Acos(dotACn);
            double c = Math.Acos(dotBCn);

            /*double[] anglesSort = new double[] { a, b, c };
            double[] angles = new double[] { a, b, c };
            int[] idx = new int[3]{0, 1, 2};
            Vector[] vecArr = new Vector[] { OAn, OBn, OCn };
            Array.Sort(anglesSort, idx);
            Debug.WriteLine("(a,b,c)=({0},{1},{2})", a * 180 / Math.PI, b * 180 / Math.PI, c * 180 / Math.PI);
            Debug.WriteLine("sorted : (a,b,c)=({0},{1},{2})", anglesSort[0] * 180 / Math.PI, anglesSort[1] * 180 / Math.PI, anglesSort[2] * 180 / Math.PI);
            Debug.WriteLine("index : {0}, {1}, {2}", idx[0], idx[1], idx[2]);
            if (anglesSort[1] > Math.PI / 2 && anglesSort[0] < Math.PI / 2)
            {
                double theta = anglesSort[1] - Math.PI / 2 + 0.001;
                var rotMatrix = new RotateTransform(theta*180/Math.PI).Value;
                vecArr[idx[1]] = rotMatrix.Transform(vecArr[idx[1]]);
                angles[idx[1]] = Math.PI/2 - 0.001;
                angles[idx[0]] = angles[idx[2]] - angles[idx[1]];
            }
            Debug.WriteLine("after : (a,b,c)=({0},{1},{2},{3})", angles[0] * 180 / Math.PI, angles[1] * 180 / Math.PI, angles[2] * 180 / Math.PI, (angles[0]+angles[1])*180/Math.PI);

            dotABn = Math.Cos(angles[0]);
            dotACn = Math.Cos(angles[1]);
            dotBCn = Math.Cos(angles[2]);

            OAn = vecArr[0].Normalized();
            OBn = vecArr[1].Normalized();
            OCn = vecArr[2].Normalized();*/
            Debug.WriteLine("Cubic Corner Index:{0}", snappedPrimitive.CubicCornerIdx);
            
            double pn, qn, rn;
            int signp = 0, signq = 0, signr = 0;
            if (dotABn < 0 && dotACn < 0 && dotBCn < 0)
            {
                signp = signq = signr = 1;
            }
            else
            {
                if (dotABn < 0)
                {
                    signp = signq = -1;
                    signr = 1;
                }
                else if (dotBCn < 0)
                {
                    signq = signr = -1;
                    signp = 1;
                }
                else
                {
                    signp = signr = -1;
                    signq = 1;
                }
            }
            
            pn = signp*Math.Sqrt(-dotACn * dotABn / dotBCn);
            qn = signq*Math.Sqrt(-dotABn * dotBCn / dotACn);
            rn = signr*Math.Sqrt(-dotACn * dotBCn / dotABn);

            Debug.WriteLine("p*q={0}, OA*OB={1}", pn * qn, dotABn);
            Debug.WriteLine("p*r={0}, OA*OC={1}", pn * rn, dotACn);
            Debug.WriteLine("q*r={0}, OB*OC={1}", rn * qn, dotBCn);

            Vector3D OA3Dn = new Vector3D(OAn.X, OAn.Y, pn);
            Vector3D OB3Dn = new Vector3D(OBn.X, OBn.Y, qn);
            Vector3D OC3Dn = new Vector3D(OCn.X, OCn.Y, rn);

            OA3Dn = OA3Dn.Normalized();
            OB3Dn = OB3Dn.Normalized();
            OC3Dn = OC3Dn.Normalized();
            Vector3D pOA = (Ap - Op).Normalized();
            Vector3D pOB = (Bp - Op).Normalized();
            Vector3D pOC = (Cp - Op).Normalized();
            Vector3D approxW = new Vector3D();// = OA3Dn.Normalized();
            Vector3D approxH = new Vector3D();// = -OB3Dn.Normalized();
            Vector3D approxD = new Vector3D();// = OC3Dn.Normalized();
            
            switch (snappedPrimitive.CubicCornerIdx)
            {
                case 0:
                    approxW = Vector3D.DotProduct(pOA, OA3Dn) > 0 ? OA3Dn : -OA3Dn;
                    approxH = Vector3D.DotProduct(pOB, OB3Dn) < 0 ? OB3Dn : -OB3Dn;
                    approxD = Vector3D.DotProduct(pOC, OC3Dn) < 0 ? OC3Dn : -OC3Dn;
                    break;
                case 1:
                    approxW = Vector3D.DotProduct(pOA, OA3Dn) > 0 ? OA3Dn : -OA3Dn;
                    approxH = Vector3D.DotProduct(pOB, OB3Dn) > 0 ? OB3Dn : -OB3Dn;
                    approxD = Vector3D.DotProduct(pOC, OC3Dn) < 0 ? OC3Dn : -OC3Dn;
                    break;
                case 2:
                    approxW = Vector3D.DotProduct(pOA, OA3Dn) > 0 ? OA3Dn : -OA3Dn;
                    approxH = Vector3D.DotProduct(pOB, OB3Dn) > 0 ? OB3Dn : -OB3Dn;
                    approxD = Vector3D.DotProduct(pOC, OC3Dn) < 0 ? OC3Dn : -OC3Dn;
                    break;
                case 3:
                    approxW = -OA3Dn.Normalized();
                    approxH = -OB3Dn.Normalized();                    
                    approxD = -OC3Dn.Normalized();
                    break;
                case 4:
                    approxW = OA3Dn.Normalized();
                    approxH = OB3Dn.Normalized();                    
                    approxD = OC3Dn.Normalized();
                    break;
                case 5:
                    approxW = Vector3D.DotProduct(pOA, OA3Dn) > 0 ? -OA3Dn : OA3Dn;
                    approxH = Vector3D.DotProduct(pOB, OB3Dn) > 0 ? OB3Dn : OB3Dn;
                    approxD = Vector3D.DotProduct(pOC, OC3Dn) < 0 ? OC3Dn : -OC3Dn;
                    break;
                case 6:
                    approxW = OA3Dn.Normalized();
                    approxH = OB3Dn.Normalized();                    
                    approxD = -OC3Dn.Normalized();
                    break;
                case 7:
                    approxW = -OA3Dn.Normalized();
                    approxH = OB3Dn.Normalized();                    
                    approxD = -OC3Dn.Normalized();
                    break;
            }

            snappedPrimitive.W = OA3Dn.Normalized();
            snappedPrimitive.H = OB3Dn.Normalized();
            snappedPrimitive.D = OC3Dn.Normalized();

            Debug.WriteLine("Normalized W*H={0}", Vector3D.DotProduct(approxW, approxH));
            Debug.WriteLine("Normalized W*D={0}", Vector3D.DotProduct(approxW, approxD));
            Debug.WriteLine("Normalized D*H={0}", Vector3D.DotProduct(approxD, approxH));

            /*double lOA = OA.Length;
            double lOB = OB.Length;
            double lOC = OC.Length;
            Debug.WriteLine("(lOA, lOB, lOC)=({0},{1},{2})", lOA, lOB, lOC);*/
            
            //a = Math.Acos(OAn * OBn);
            //b = Math.Acos(OAn * OCn);
            //c = Math.Acos(OBn * OCn);
            //Debug.WriteLine("(pi/2, a,b,c,sum)=({0},{1},{2}, {3},{4})", Math.PI / 2, a * 180 / Math.PI, b * 180 / Math.PI, c * 180 / Math.PI, a * 180 / Math.PI + b * 180 / Math.PI);
            /*double cota = 1 / Math.Tan(a);
            double cotb = 1 / Math.Tan(b);
            double cotc = 1 / Math.Tan(c);*/

            /*double dotAB = lOA * lOB * Math.Cos(a);
            double dotAC = lOA * lOC * Math.Cos(b);
            double dotBC = lOB * lOC * Math.Cos(c);*/

            double dotAB = OA * OB;
            double dotAC = OA * OC;
            double dotBC = OB * OC;
            double p, q, r;
            p = signp * Math.Sqrt(-dotAC * dotAB / dotBC);
            q = signq * Math.Sqrt(-dotAB * dotBC / dotAC);
            r = signr * Math.Sqrt(-dotAC * dotBC / dotAB);

            //double r = Math.Sqrt(-dotAB*dotBC/dotAC);
            //Debug.WriteLine("(p,q,r)=({0},{1},{2})", p, q, r);

            //ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[0];
            Vector3D cuboidOA = ec.Points3D[1] - ec.Points3D[0];
            //ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[1];
            Vector3D cuboidOB = ec.Points3D[1] - ec.Points3D[0];
            //ec = (EnhancedPrimitiveCurve)snappedPrimitive.CubicCorner[2];
            Vector3D cuboidOC = ec.Points3D[1] - ec.Points3D[0];
            double zAp = p;
            double zAn = -zAp;
            double zBp = q; 
            double zBn = -zBp;
            double zCp = r;
            double zCn = -zCp;
            
            Point3D O3D = new Point3D(O.X, O.Y, 0);
            snappedPrimitive.Origin = O3D;
            Point3D A3Dp = new Point3D(A.X, A.Y, zAp);
            Point3D A3Dn = new Point3D(A.X, A.Y, -zAp);
            Vector3D OA3Dp = A3Dp - O3D;
            //Point3D A3D = (Vector3D.DotProduct(cuboidOA, OA3Dp) > 0) ? A3Dp : A3Dn;
            Point3D A3D = A3Dp;// (Vector3D.DotProduct(cuboidOA, OA3Dp) > 0) ? A3Dp : A3Dn;
            //Vector

            Point3D B3Dp = new Point3D(B.X, B.Y, zBp);
            Point3D B3Dn = new Point3D(B.X, B.Y, -zBp);
            Vector3D OB3Dp = B3Dp - O3D;
            //Point3D B3D = (Vector3D.DotProduct(cuboidOB, OB3Dp) > 0) ? B3Dp : B3Dn;
            Point3D B3D = B3Dp; //(Vector3D.DotProduct(cuboidOB, OB3Dp) > 0) ? B3Dp : B3Dn;
            
            Point3D C3Dp = new Point3D(C.X, C.Y, zCp);
            Point3D C3Dn = new Point3D(C.X, C.Y, -zCp);
            Vector3D OC3Dp = C3Dp - O3D;
            //Point3D C3D = (Vector3D.DotProduct(cuboidOC, OC3Dp) > 0) ? C3Dp : C3Dn;
            Point3D C3D = C3Dp;

            Debug.WriteLine("OA3D=({0},{1},{2})", OA3Dp.X, OA3Dp.Y, OA3Dp.Z);
            Debug.WriteLine("OB3D=({0},{1},{2})", OB3Dp.X, OB3Dp.Y, OB3Dp.Z);
            
            Debug.WriteLine("W*H={0}", Vector3D.DotProduct(approxH, approxW));
            Debug.WriteLine("W*D={0}", Vector3D.DotProduct(approxD, approxW));
            Debug.WriteLine("D*H={0}", Vector3D.DotProduct(approxH, approxD));

            double approxWidth = (A3D - O3D).Length;
            double approxHeight = (O3D - B3D).Length;
            double approxDepth = (C3D - O3D).Length;
            Point3D approxCenter = new Point3D();
            switch (snappedPrimitive.CubicCornerIdx)
            {
                case 0:
                    approxCenter = O3D + 0.5 * approxWidth * approxW - 0.5 * approxHeight * approxH + 0.5 * approxDepth * approxD;
                    break;
                case 1:
                    approxCenter = O3D - 0.5 * approxWidth * approxW - 0.5 * approxHeight * approxH + 0.5 * approxDepth * approxD;
                    break;
                case 2:
                    approxCenter = O3D + 0.5 * approxWidth * approxW - 0.5 * approxHeight * approxH - 0.5 * approxDepth * approxD;
                    break;
                case 3:
                    approxCenter = O3D - 0.5 * approxWidth * approxW - 0.5 * approxHeight * approxH - 0.5 * approxDepth * approxD;
                    break;
                case 4:
                    approxCenter = O3D + 0.5 * approxWidth * approxW + 0.5 * approxHeight * approxH + 0.5 * approxDepth * approxD;
                    break;
                case 5:
                    approxCenter = O3D - 0.5 * approxWidth * approxW + 0.5 * approxHeight * approxH + 0.5 * approxDepth * approxD;
                    break;
                case 6:
                    approxCenter = O3D + 0.5 * approxWidth * approxW + 0.5 * approxHeight * approxH - 0.5 * approxDepth * approxD;
                    break;
                case 7:
                    approxCenter = O3D - 0.5 * approxWidth * approxW + 0.5 * approxHeight * approxH - 0.5 * approxDepth * approxD;
                    break;
            }
            var CenterTerm = 
                TermBuilder.Power(snappedPrimitive.Center.X - approxCenter.X, 2) +
                TermBuilder.Power(snappedPrimitive.Center.Y - approxCenter.Y, 2) +
                TermBuilder.Power(snappedPrimitive.Center.Z - approxCenter.Z, 2);
            
            var LengthTerm =
                TermBuilder.Power(snappedPrimitive.Width - approxWidth, 2) +
                TermBuilder.Power(snappedPrimitive.Height - approxHeight, 2) +
                TermBuilder.Power(snappedPrimitive.Depth - approxDepth, 2);

            var WidthVectorTerm = 10*
                TermBuilder.Power(snappedPrimitive.Wv.X - approxW.X, 2) +
                TermBuilder.Power(snappedPrimitive.Wv.Y - approxW.Y, 2) +
                TermBuilder.Power(snappedPrimitive.Wv.Z - approxW.Z, 2);

            var HeightVectorTerm = 10*
                TermBuilder.Power(snappedPrimitive.Hv.X - approxH.X, 2) +
                TermBuilder.Power(snappedPrimitive.Hv.Y - approxH.Y, 2) +
                TermBuilder.Power(snappedPrimitive.Hv.Z - approxH.Z, 2);

            var DepthVectorTerm = 10*
                TermBuilder.Power(snappedPrimitive.Dv.X - approxD.X, 2) +
                TermBuilder.Power(snappedPrimitive.Dv.Y - approxD.Y, 2) +
                TermBuilder.Power(snappedPrimitive.Dv.Z - approxD.Z, 2);

            var objective =
                CenterTerm +
                LengthTerm +
                WidthVectorTerm +
                HeightVectorTerm +
                DepthVectorTerm;

            var ABorthogonal = TVec.InnerProduct(snappedPrimitive.Wv, snappedPrimitive.Hv);
            var ACorthogonal = TVec.InnerProduct(snappedPrimitive.Wv, snappedPrimitive.Dv);
            var BCorthogonal = TVec.InnerProduct(snappedPrimitive.Hv, snappedPrimitive.Wv);
            var constraints = new Term[] { snappedPrimitive.Wv.NormSquared - 1, snappedPrimitive.Hv.NormSquared - 1, snappedPrimitive.Dv.NormSquared - 1 , ABorthogonal, ACorthogonal, BCorthogonal };

            return Tuple.Create(objective, constraints);
        }
    }
}
