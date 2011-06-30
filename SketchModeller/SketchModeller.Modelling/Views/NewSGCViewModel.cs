using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Views
{
    class NewSGCViewModel : NewPrimitiveViewModel
    {
        private NewStraightGenCylinder model;

        public NewSGCViewModel(
            UiState uiState = null, 
            ICurveAssigner curveAssigner = null, 
            IEventAggregator eventAggregator = null)
            : base(uiState, curveAssigner, eventAggregator)
        {
            model = new NewStraightGenCylinder();
        }

        public override void UpdateFromModel()
        {
            throw new NotImplementedException();
        }
    }
}
