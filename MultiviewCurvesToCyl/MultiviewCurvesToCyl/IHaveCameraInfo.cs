using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace MultiviewCurvesToCyl
{
    interface IHaveCameraInfo
    {
        Point3D ViewPosition { get; }
        Vector3D ViewDirection { get; }
        Vector3D UpDirection { get; }
        Matrix3D TotalCameraMatrix { get; }
    }

}
