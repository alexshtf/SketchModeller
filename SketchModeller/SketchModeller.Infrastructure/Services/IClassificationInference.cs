﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Services
{
    public interface IClassificationInference
    {
        void PreAnalyze();
        void Infer();
    }
}
