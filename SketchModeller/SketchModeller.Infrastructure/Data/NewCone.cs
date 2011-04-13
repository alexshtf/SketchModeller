using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewCone : NewCylindricalPrimitive
    {
        #region TopRadius property

        private double topRadius;

        public double TopRadius
        {
            get { return topRadius; }
            set
            {
                topRadius = value;
                RaisePropertyChanged(() => TopRadius);
            }
        }

        #endregion

        #region BottomRadius property

        private double bottomRadius;

        public double BottomRadius
        {
            get { return bottomRadius; }
            set
            {
                bottomRadius = value;
                RaisePropertyChanged(() => BottomRadius);
            }
        }

        #endregion

        protected override double TopRadiusInternal
        {
            get { return TopRadius; }
        }

        protected override double BottomRadiusInternal
        {
            get { return BottomRadius; }
        }
    }
}
