using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    public class Parallelism : Annotation
    {
        public PointsSequence[] Elements { get; set; }

        public override Annotation Clone()
        {
            return new Parallelism { Elements = (PointsSequence[])Elements.Clone() };
        }
    }
}
