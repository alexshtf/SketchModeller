﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class SnappedPrimitive : NotificationObject
    {
        public PointsSequence[] SnappedTo { get; set; }
    }
}
