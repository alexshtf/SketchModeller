using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using MultiviewCurvesToCyl.Base;

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// View model for newly created cyllinders, before any snapping began.
    /// </summary>
    class NewCylinderViewModel : Base3DViewModel
    {
        private const double INITIAL_LENGTH = 100;
        private const double INITIAL_RADIUS = 50;
        private static readonly Vector3D INITIAL_ORIENTATION = new Vector3D(1, 0, 0);

        private Point3D center;
        private Vector3D orientation;
        private double length;
        private double radius;

        [ContractInvariantMethod]
        private void ContractInvariants()
        {
            Contract.Invariant(orientation.LengthSquared > 0);
            Contract.Invariant(length > 0);
            Contract.Invariant(radius > 0);
            Contract.Invariant(center.IsFinite());
        }

        public NewCylinderViewModel()
        {
            orientation = INITIAL_ORIENTATION;
            length = INITIAL_LENGTH;
            radius = INITIAL_RADIUS;
        }

        public Point3D Center
        {
            get { return center; }
            set
            {
                Contract.Requires(value.IsFinite());

                center = value;
                NotifyPropertyChanged(() => Center);
            }
        }

        public Vector3D Orientation
        {
            get { return orientation; }
            set
            {
                Contract.Requires(value.LengthSquared > 0);

                orientation = value;
                NotifyPropertyChanged(() => Orientation);
            }
        }

        public double Length
        {
            get { return length; }
            set
            {
                Contract.Requires(value > 0);

                length = value;
                NotifyPropertyChanged(() => Length);
            }
        }


        public double Radius
        {
            get { return radius; }
            set
            {
                Contract.Requires(value > 0);

                radius = value;
                NotifyPropertyChanged(() => Radius);
            }
        }
    }
}
