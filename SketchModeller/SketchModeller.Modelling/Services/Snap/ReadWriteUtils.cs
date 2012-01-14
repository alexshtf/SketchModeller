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
                //.Write(bgc.TopCenterResult)
                .Write(bgc.NPtopResult)
                .Write(bgc.NPbotResult)
                .Write(bgc.Uresult)
                .Write(bgc.Vresult);

            foreach (var component in bgc.ComponentResults)
                writer = writer
                    .Write(component.Radius)
                    .Write(component.S)
                    .Write(component.T);

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
                .Write(bgc.NPtop)
                .Write(bgc.NPbot)
                .Write(bgc.U)
                .Write(bgc.V);

            foreach (var component in bgc.Components)
                writer = writer
                    .Write(component.Radius)
                    .Write(component.vS)
                    .Write(component.vT);

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
            //bgc.TopCenterResult = reader.ReadPoint3D();
            bgc.NPtopResult = reader.ReadVector();
            bgc.NPbotResult= reader.ReadVector();
            bgc.Uresult = reader.ReadVector3D();
            bgc.Vresult = reader.ReadVector3D();

            foreach (var i in Enumerable.Range(0, bgc.ComponentResults.Length))
                bgc.ComponentResults[i] =
                     new BendedCylinderComponent(reader.ReadValue(), bgc.ComponentResults[i].Progress, reader.ReadValue(), reader.ReadValue());
        }

        #endregion
    }
}
