using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SketchModeller.Infrastructure.Data
{
    [DebuggerDisplay("({x}, {y}, {z})")]
    public class Point3D : NotificationObject
    {
        #region X property

        private double x;

        [XmlAttribute]
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                RaisePropertyChanged("X");
            }
        }

        #endregion

        #region Y property

        private double y;

        [XmlAttribute]
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                RaisePropertyChanged("Y");
            }
        }

        #endregion

        #region Z property

        private double z;
        
        [XmlAttribute]
        public double Z
        {
            get { return z; }
            set
            {
                z = value;
                RaisePropertyChanged("Z");
            }
        }

        #endregion
    }
}
