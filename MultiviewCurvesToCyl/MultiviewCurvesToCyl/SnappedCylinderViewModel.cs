using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using MultiviewCurvesToCyl.MeshGeneration;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderViewModel : BaseEditorObjectViewModel
    {
        public const double INITIAL_LENGTH = 5;
        public const double INITIAL_RADIUS = 5;
        public static readonly Vector3D INITIAL_ORIENTATION = MathUtils3D.UnitX;
        public static readonly Vector3D INITIAL_VIEW_DIRECTION = MathUtils3D.UnitY;
        public static readonly Point3D INITIAL_CENTER = new Point3D();

        public SnappedCylinderViewModel()
        {
            // we perform initial initialization such that in designer mode we will be able to create new instances of the
            // view model without needing special initialization.
            CylinderData = new ConstrainedCylinder(
                INITIAL_RADIUS, 
                INITIAL_LENGTH, 
                INITIAL_CENTER, 
                INITIAL_ORIENTATION, 
                INITIAL_VIEW_DIRECTION);
        }


        public ConstrainedCylinder CylinderData { get; private set;}

        public bool IsInitialized { get ; private set; }

        public void Initialize(double radius, double length, Point3D center, Vector3D orientation, Vector3D viewDirection)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Cannot initialize the object twice");

            CylinderData = new ConstrainedCylinder(radius, length, center, orientation, viewDirection);
            IsInitialized = true;
        }
    }
}
