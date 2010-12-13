using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Data
{
    public class Point : NotificationObject
    {
        private double x;
        private double y;

        public double X
        {
            get { return x; }
            set
            {
                x = value;
                RaisePropertyChanged("X");
            }
        }

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
