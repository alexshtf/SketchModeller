using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Commands;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Views
{
    public class CurvesToolbarViewModel : NotificationObject
    {
        private readonly SessionData sessionData;

        public CurvesToolbarViewModel()
        {
        }

        [InjectionConstructor]
        public CurvesToolbarViewModel(SessionData sessionData)
            : this()
        {
            this.sessionData = sessionData;
            MarkFeature = new DelegateCommand(MarkFeatureExecute);
            MarkSilhouette = new DelegateCommand(MarkSilhouetteExecute);
        }

        public ICommand MarkFeature { get; private set; }
        public ICommand MarkSilhouette { get; private set; }

        private void MarkFeatureExecute()
        {
            MarkAs(CurveCategories.Feature);
        }

        private void MarkSilhouetteExecute()
        {
            MarkAs(CurveCategories.Silhouette);
        }

        private void MarkAs(CurveCategories newCategory)
        {
            foreach (var curve in sessionData.SelectedSketchObjects)
                curve.CurveCategory = newCategory;
        }

    }
}
