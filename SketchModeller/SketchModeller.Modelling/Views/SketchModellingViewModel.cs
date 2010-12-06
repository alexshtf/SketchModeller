using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Unity;
using System.Windows;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Modelling.Views
{
    public class SketchModellingViewModel : NotificationObject, IWeakEventListener
    {
        private UiState uiState;
        private IUnityContainer container;

        public SketchModellingViewModel()
        {
            NewPrimitiveViewModels = new ObservableCollection<NewPrimitiveViewModel>();
            sketchPlane = SketchPlane.Default;
        }

        public SketchModellingViewModel(UiState uiState, IUnityContainer container)
            : this()
        {
            this.uiState = uiState;
            this.container = container;

            uiState.AddListener(this, () => uiState.SketchPlane);
            uiState.AddListener(this, () => uiState.ImageWidth);
            uiState.AddListener(this, () => uiState.ImageHeight);

            sketchPlane = uiState.SketchPlane;
        }

        public ObservableCollection<NewPrimitiveViewModel> NewPrimitiveViewModels { get; private set; }

        #region SketchPlane property

        private SketchPlane sketchPlane;

        public SketchPlane SketchPlane
        {
            get { return sketchPlane; }
            set
            {
                sketchPlane = value;
                RaisePropertyChanged(() => SketchPlane);
            }
        }

        #endregion

        #region ImageWidth property

        private double imageWidth;

        public double ImageWidth
        {
            get { return imageWidth; }
            set
            {
                imageWidth = value;
                RaisePropertyChanged(() => ImageWidth);
            }
        }

        #endregion

        #region ImageHeight property

        private double imageHeight;

        public double ImageHeight
        {
            get { return imageHeight; }
            set
            {
                imageHeight = value;
                RaisePropertyChanged(() => ImageHeight);
            }
        }

        #endregion

        public void TryAddNewPrimitive(Point3D point3d)
        {
            if (uiState.Tool == Tool.InsertCylinder)
            {
                var viewModel = container.Resolve<NewCylinderViewModel>();
                viewModel.Initialize(point3d);
                NewPrimitiveViewModels.Add(new NewCylinderViewModel());
            }
            uiState.Tool = Tool.Manipulation;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;
             
            var eventArgs = (PropertyChangedEventArgs)e;
            if (eventArgs.Match(() => uiState.SketchPlane))
                SketchPlane = uiState.SketchPlane;

            if (eventArgs.Match(() => uiState.ImageWidth))
                ImageWidth = uiState.ImageWidth;
            if (eventArgs.Match(() => uiState.ImageHeight))
                ImageHeight = uiState.ImageHeight;

            return true;
        }
    }
}
