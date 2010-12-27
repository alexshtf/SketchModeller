using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Media.Media3D;
using Utils;

namespace SketchModeller.Modelling.ModelViews
{
    public class ModelViewerViewModel : NotificationObject
    {
        private const double MOVE_SPEED = 0.01;
        private readonly UiState uiState;

        public ModelViewerViewModel()
        {
        }

        [InjectionConstructor]
        public ModelViewerViewModel(UiState uiState)
        {
            MoveForward = new DelegateCommand(() =>Move(MathUtils3D.UnitZ));
            MoveBack = new DelegateCommand(() => Move(-MathUtils3D.UnitZ));
            MoveLeft = new DelegateCommand(() => Move(-MathUtils3D.UnitX));
            MoveRight = new DelegateCommand(() => Move(MathUtils3D.UnitX));
        }

        public ICommand MoveForward { get; private set; }
        public ICommand MoveBack { get; private set; }
        public ICommand MoveLeft { get; private set; }
        public ICommand MoveRight { get; private set; }

        public ICommand LookUp { get; private set; }
        public ICommand LookDown { get; private set; }
        public ICommand LookLeft { get; private set; }
        public ICommand LookRight { get; private set; }

        #region Position property

        private Point3D position;

        public Point3D Position
        {
            get { return position; }
            set
            {
                position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        #endregion

        #region LookDirection property

        private Vector3D lookDirection;

        public Vector3D LookDirection
        {
            get { return lookDirection; }
            set
            {
                lookDirection = value;
                RaisePropertyChanged(() => LookDirection);
                RaisePropertyChanged(() => RightDirection);
            }
        }

        #endregion

        #region UpDirection property

        private Vector3D upDirection;

        public Vector3D UpDirection
        {
            get { return upDirection; }
            set
            {
                upDirection = value;
                RaisePropertyChanged(() => UpDirection);
                RaisePropertyChanged(() => RightDirection);
            }
        }

        #endregion

        #region ForwardDirection property

        public Vector3D RightDirection
        {
            get { return Vector3D.CrossProduct(LookDirection, UpDirection); }
        }

        #endregion

        private void Move(Vector3D vector3D)
        {
            var xAxis = RightDirection.Normalized();
            var yAxis = upDirection.Normalized();
            var zAxis = lookDirection.Normalized();

            var moveVector = vector3D.X * xAxis + vector3D.Y * yAxis + vector3D.Z * zAxis;
            Position = Position + MOVE_SPEED * moveVector;
        }
    }
}
