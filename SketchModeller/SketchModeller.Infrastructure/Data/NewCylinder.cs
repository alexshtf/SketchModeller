using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewCylinder : NewCylindricalPrimitive
    {
        #region Diameter property

        private double diameter;

        public double Diameter
        {
            get { return diameter; }
            set
            {
                diameter = value;
                RaisePropertyChanged(() => Diameter);
            }
        }

        #endregion

        public double Radius
        {
            get { return Diameter / 2; }
        }

        protected override double BottomRadiusInternal
        {
            get { return Radius; }
        }

        protected override double TopRadiusInternal
        {
            get { return Radius; }
        }
    }
}
