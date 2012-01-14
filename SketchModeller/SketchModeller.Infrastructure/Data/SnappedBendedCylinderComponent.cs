using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedBendedCylinderComponent : SnappedCyliderComponent
    {
        private readonly Variable vs;
        private readonly Variable vt;
        public SnappedBendedCylinderComponent(Variable radius, double progress, Variable vs, Variable vt)
            : base(radius, progress)
        {
            this.vs = vs;
            this.vt = vt;
        }
        public Variable vS
        {
            get { return vs; }
        }
        public Variable vT
        {
            get { return vt; }
        }
    }
}
