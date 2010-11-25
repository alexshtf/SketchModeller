using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data
{
    public class Segment : NotificationObject
    {
        private Point start;
        private Point end;

        public Segment()
        {
            start = new Point();
            end = new Point();
        }

        [ContractInvariantMethod]
        private void InvariantsMethod()
        {
            Contract.Invariant(start != null);
            Contract.Invariant(end != null);
        }

        public Point Start
        {
            get { return start; }
            set
            {
                Contract.Requires(value != null);

                start = value;
                RaisePropertyChanged(() => Start);
            }
        }

        public Point End
        {
            get { return end; }
            set
            {
                Contract.Requires(value != null);

                end = value;
                RaisePropertyChanged(() => End);
            }
        }
    }
}
