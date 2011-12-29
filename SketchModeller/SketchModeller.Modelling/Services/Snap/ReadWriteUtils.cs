using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Utilities;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;
using System.Windows;
namespace SketchModeller.Modelling.Services.Snap
{
    static class ReadWriteUtils
    {
        #region SnappedCylinder methods

        public static VectorsWriter Write(this VectorsWriter writer, SnappedCylinder cylinder)
        {
            return writer
                .Write(cylinder.BottomCenterResult)
                .Write(cylinder.AxisResult)
                .Write(cylinder.LengthResult)
                .Write(cylinder.RadiusResult);
        }

        public static VariableVectorsWriter Write(this VariableVectorsWriter writer, SnappedCylinder cylinder)
        {
            return writer  
                .Write(cylinder.BottomCenter)
                .Write(cylinder.Axis)
                .Write(cylinder.Length)
                .Write(cylinder.Radius);
        }

        public static void Read(this VectorsReader reader, SnappedCylinder cylinder)
        {
            cylinder.BottomCenterResult = reader.ReadPoint3D();
            cylinder.AxisResult = reader.ReadVector3D();
            cylinder.LengthResult = reader.ReadValue();
            cylinder.RadiusResult = Math.Abs(reader.ReadValue());
        }
        
        #endregion

        #region SnappedCone methods

        public static VectorsWriter Write(this VectorsWriter writer, SnappedCone cone)
        {
            return writer
                .Write(cone.BottomCenterResult)
                .Write(cone.AxisResult)
                .Write(cone.LengthResult)
                .Write(cone.TopRadiusResult)
                .Write(cone.BottomRadiusResult);
        }

        public static VariableVectorsWriter Write(this VariableVectorsWriter writer, SnappedCone cone)
        {
            return writer
                .Write(cone.BottomCenter)
                .Write(cone.Axis)
                .Write(cone.Length)
                .Write(cone.TopRadius)
                .Write(cone.BottomRadius);
        }

        public static void Read(this VectorsReader reader, SnappedCone cone)
        {
            cone.BottomCenterResult = reader.ReadPoint3D();
            cone.AxisResult = reader.ReadVector3D();
            cone.LengthResult = reader.ReadValue();
            cone.TopRadiusResult = Math.Abs(reader.ReadValue());
            cone.BottomRadiusResult = Math.Abs(reader.ReadValue());
        }
        
        #endregion

        #region SnappedSphere methods

        public static VectorsWriter Write(this VectorsWriter writer, SnappedSphere sphere)
        {
            return writer
                .Write(sphere.CenterResult)
                .Write(sphere.RadiusResult);
        }

        public static VariableVectorsWriter Write(this VariableVectorsWriter writer, SnappedSphere sphere)
        {
            return writer
                .Write(sphere.Center)
                .Write(sphere.Radius);
        }

        public static void Read(this VectorsReader reader, SnappedSphere sphere)
        {
            sphere.CenterResult = reader.ReadPoint3D();
            sphere.RadiusResult = reader.ReadValue();
        }

        #endregion

        #region SnappedStraightGenCylinder methods

        public static VectorsWriter Write(this VectorsWriter writer, SnappedStraightGenCylinder sgc)
        {
            writer = writer
                .Write(sgc.BottomCenterResult)
                .Write(sgc.AxisResult)
                .Write(sgc.LengthResult);
            foreach (var component in sgc.ComponentResults)
                writer = writer
                    .Write(component.Radius);

            return writer;
        }

        public static VectorsWriter Write(this VectorsWriter writer, SnappedBendedGenCylinder bgc)
        {
            writer = writer
                .Write(bgc.BottomCenterResult)
                .Write(bgc.AxisResult)
                .Write(bgc.LengthResult);
            foreach (var component in bgc.ComponentResults)
                writer = writer
                    .Write(component.Radius)
                    .Write(component.Pnt2D);

            return writer;
        }

        public static VariableVectorsWriter Write(this VariableVectorsWriter writer, SnappedStraightGenCylinder sgc)
        {
            writer = writer
                .Write(sgc.BottomCenter)
                .Write(sgc.Axis)
                .Write(sgc.Length);

            foreach (var component in sgc.Components)
                writer = writer
                    .Write(component.Radius);

            return writer;
        }

        public static VariableVectorsWriter Write(this VariableVectorsWriter writer, SnappedBendedGenCylinder bgc)
        {
            writer = writer
                .Write(bgc.BottomCenter)
                .Write(bgc.Axis)
                .Write(bgc.Length);

            foreach (var component in bgc.Components)
                writer = writer
                    .Write(component.Radius)
                    .Write(component.PntOnSpine);

            return writer;
        }

        public static void Read(this VectorsReader reader, SnappedStraightGenCylinder sgc)
        {
            sgc.BottomCenterResult = reader.ReadPoint3D();
            sgc.AxisResult = reader.ReadVector3D();
            sgc.LengthResult = reader.ReadValue();

            foreach (var i in Enumerable.Range(0, sgc.ComponentResults.Length))
                sgc.ComponentResults[i] = 
                    new CylinderComponent(reader.ReadValue(), sgc.ComponentResults[i].Progress);
        }

        public static void Read(this VectorsReader reader, SnappedBendedGenCylinder bgc)
        {
            bgc.BottomCenterResult = reader.ReadPoint3D();
            bgc.AxisResult = reader.ReadVector3D();
            bgc.LengthResult = reader.ReadValue();
            var Axis = bgc.AxisResult;
            Axis.Normalize();
            //Axis = new Vector3D(0.0, 1.0, 0.0);
            //bgc.AxisResult = Axis;
            MessageBox.Show(String.Format("{0},{1},{2}", Axis.X, Axis.Y, Axis.Z));
            foreach (var i in Enumerable.Range(0, bgc.ComponentResults.Length))
            {   
                var radius = reader.ReadValue();
                var pnt2D = reader.ReadPoint();
                bgc.ComponentResults[i] =
                    new BendedCylinderComponent(radius, bgc.ComponentResults[i].Progress, new Point3D(bgc.BottomCenterResult.X + bgc.LengthResult * bgc.ComponentResults[i].Progress*Axis.X,
                                                                                                      bgc.BottomCenterResult.Y + bgc.LengthResult * bgc.ComponentResults[i].Progress * Axis.Y,
                                                                                                      bgc.BottomCenterResult.Z + bgc.LengthResult * bgc.ComponentResults[i].Progress * Axis.Z), pnt2D);
            }
            bgc.ComponentResults[0].Pnt3D = bgc.BottomCenterResult;
            
            Vector3D ProjXZ = new Vector3D(Axis.X, 0, Axis.Z);
            double d = ProjXZ.Length;
            double anglexz = 0;
            if (d > 0) anglexz = 180 * Math.Acos(Axis.X / d) / Math.PI;
            var rotationxz = Matrix3D.Identity;
            rotationxz.Rotate(new Quaternion(new Vector3D(0.0, 1.0, 0.0), anglexz));

            Vector3D ProjYZ = new Vector3D(0.0, Axis.Y, Axis.Z);
            d = Math.Sqrt(Math.Pow(ProjYZ.Y, 2) + Math.Pow(ProjYZ.Z, 2));
            double angleyz = 0.0;
            if (d > 0) angleyz = 180 * Math.Acos(Axis.Y / d) / Math.PI;
            var rotationyz = Matrix3D.Identity;
            rotationyz.Rotate(new Quaternion(new Vector3D(1.0, 0.0, 0.0), angleyz));

            foreach (var i in Enumerable.Range(1, bgc.ComponentResults.Length-1))
            {
                Vector vspine = new Vector(bgc.ComponentResults[i].Pnt2D.X - bgc.ComponentResults[i - 1].Pnt2D.X, bgc.ComponentResults[i].Pnt2D.Y - bgc.ComponentResults[i - 1].Pnt2D.Y);
                    
                var XZspine = rotationxz.Transform(new Vector3D(vspine.X, 0.0, 0.0));
                var YZspine = rotationyz.Transform(new Vector3D(0.0, vspine.Y, 0.0));
                Vector3D Test = new Vector3D(XZspine.X, YZspine.Y, XZspine.Z  + YZspine.Z);
                MessageBox.Show(String.Format("Actual Length={0}, Computed Length={1}, ", vspine.Length, Test.Length));
           
                bgc.ComponentResults[i].Pnt3D = new Point3D(bgc.ComponentResults[i - 1].Pnt3D.X + XZspine.X, 
                                                            bgc.ComponentResults[i - 1].Pnt3D.Y + YZspine.Y, 
                                                            bgc.ComponentResults[i - 1].Pnt3D.Z + XZspine.Z  + YZspine.Z);
            }
        }

        #endregion
    }
}
