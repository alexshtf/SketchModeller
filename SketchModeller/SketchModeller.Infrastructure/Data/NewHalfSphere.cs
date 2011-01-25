using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Data
{
    public class NewHalfSphere : NewPrimitive
    {
        #region Radius property

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RaisePropertyChanged(() => Radius);
            }
        }

        #endregion

        #region Center property

        private Point3D center;

        public Point3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
            }
        }

        #endregion

        #region Axis property

        private Point3D axis;

        public Point3D Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                RaisePropertyChanged(() => Axis);
            }
        }

        #endregion

        #region Length property

        private double length;

        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                RaisePropertyChanged(() => Length);
            }
        }

        #endregion

        public override NewPrimitive Clone()
        {
            return new NewHalfSphere
            {
                Radius = radius,
                Center = center.Clone(),
                Axis = axis.Clone(),
                Length = length,
            };
        }
    }
}
