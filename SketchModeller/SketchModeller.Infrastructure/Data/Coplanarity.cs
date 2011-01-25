using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class Coplanarity : Annotation
    {
        /// <summary>
        /// Gets a list of coplanar elements
        /// </summary>
        public PointsSequence[] Elements { get; set; }

        public override Annotation Clone()
        {
            return new Coplanarity { Elements = (PointsSequence[])Elements.Clone() };
        }
    }
}
