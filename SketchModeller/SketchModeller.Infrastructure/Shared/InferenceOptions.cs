using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Shared
{
    public class InferenceOptions : NotificationObject
    {
        public InferenceOptions()
        {
            coplanarity = true;
            cocentrality = true;
            coplanarCenters = true;
            collinearCenters = true;
            orthogonalAxes = true;
            parallelism = true;
            onSphere = true;
        }

        #region Coplanarity property

        private bool coplanarity;

        public bool Coplanarity
        {
            get { return coplanarity; }
            set
            {
                coplanarity = value;
                RaisePropertyChanged(() => Coplanarity);
            }
        }

        #endregion

        #region Cocentrality property

        private bool cocentrality;

        public bool Cocentrality
        {
            get { return cocentrality; }
            set
            {
                cocentrality = value;
                RaisePropertyChanged(() => Cocentrality);
            }
        }

        #endregion

        #region CoplanarCenters property

        private bool coplanarCenters;

        public bool CoplanarCenters
        {
            get { return coplanarCenters; }
            set
            {
                coplanarCenters = value;
                RaisePropertyChanged(() => CoplanarCenters);
            }
        }

        #endregion

        #region CollinearCenters property

        private bool collinearCenters;

        public bool CollinearCenters
        {
            get { return collinearCenters; }
            set
            {
                collinearCenters = value;
                RaisePropertyChanged(() => CollinearCenters);
            }
        }

        #endregion

        #region OrthogonalAxes property

        private bool orthogonalAxes;

        public bool OrthogonalAxes
        {
            get { return orthogonalAxes; }
            set
            {
                orthogonalAxes = value;
                RaisePropertyChanged(() => OrthogonalAxes);
            }
        }

        #endregion

        #region Parallelism property

        private bool parallelism;

        public bool Parallelism
        {
            get { return parallelism; }
            set
            {
                parallelism = value;
                RaisePropertyChanged(() => Parallelism);
            }
        }

        #endregion

        #region OnSphere property

        private bool onSphere;

        public bool OnSphere
        {
            get { return onSphere; }
            set
            {
                onSphere = value;
                RaisePropertyChanged(() => OnSphere);
            }
        }

        #endregion
    }
}
