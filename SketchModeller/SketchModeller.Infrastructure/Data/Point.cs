using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SketchModeller.Infrastructure.Data
{
    [DebuggerDisplay("({x}, {y})")]
    public class Point : NotificationObject
    {
        private double x;
        private double y;

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
    }
}
