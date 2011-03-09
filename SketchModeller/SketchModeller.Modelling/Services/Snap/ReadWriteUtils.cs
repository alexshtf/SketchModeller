using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Utilities;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    static class ReadWriteUtils
    {
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
            cylinder.RadiusResult = reader.ReadValue();
        }

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
            cone.TopRadiusResult = reader.ReadValue();
            cone.BottomRadiusResult = reader.ReadValue();
        }
    }
}
