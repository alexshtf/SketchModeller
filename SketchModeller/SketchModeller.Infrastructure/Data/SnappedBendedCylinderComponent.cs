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
        private readonly TVec pntOnSpine;
        public SnappedBendedCylinderComponent(Variable radius, double progress, TVec pntOnSpine)
            : base(radius, progress)
        {
            this.pntOnSpine = pntOnSpine;
        }
        public TVec PntOnSpine
        {
            get { return pntOnSpine; }
        }
    }
}
