﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using System.Windows.Media;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerFactory : IVisual3DFactory
    {
        public static readonly ModelViewerFactory Instance = new ModelViewerFactory();

        public Visual3D Create(object item)
        {
            Visual3D result = new ModelVisual3D();
            item.MatchClass<NewCylinder>(cylinderData => result = CreateCylinderView(cylinderData));

            return result;
        }
    }
}
