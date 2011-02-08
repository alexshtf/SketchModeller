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
using System.Collections.ObjectModel;
using SnappedPrimitive = SketchModeller.Infrastructure.Data.SnappedPrimitive;
using NewPrimitive = SketchModeller.Infrastructure.Data.NewPrimitive;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Collections.Specialized;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.ModelViews
{
    public class ModelViewerViewModel : NotificationObject
    {
        private static readonly Point3D DEFAULT_POSITION = new Point3D(0, 0, 4);
        private static readonly Vector3D DEFAULT_LOOK_DIRECTION = new Vector3D(0, 0, -1);
        private static readonly Vector3D DEFAULT_UP_DIRECTION = new Vector3D(0, 1, 0);

        private const double MOVE_SPEED = 0.01;
        private const double LOOK_SPEED = 0.5;
        private const double TRACKBALL_SPEED = 0.5;
        private const double TRACKBALL_ZOOM_SPEED = 0.05;
        private readonly UiState uiState;
        private readonly SessionData sessionData;

        public ModelViewerViewModel()
        {
            isFlightMode = true;

            ResetCamera();
            Primitives = new ReadOnlyObservableCollection<NewPrimitive>(new ObservableCollection<NewPrimitive>());
            SnappedPrimitives = new ReadOnlyObservableCollection<SnappedPrimitive>(new ObservableCollection<SnappedPrimitive>());
        }

        [InjectionConstructor]
        public ModelViewerViewModel(UiState uiState, SessionData sessionData, IEventAggregator eventAggregator)
            : this()
        {
            this.uiState = uiState;
            this.sessionData = sessionData;

            MoveForward = new DelegateCommand(() => Move(MathUtils3D.UnitZ));
            MoveBack = new DelegateCommand(() => Move(-MathUtils3D.UnitZ));
            MoveLeft = new DelegateCommand(() => Move(-MathUtils3D.UnitX));
            MoveRight = new DelegateCommand(() => Move(MathUtils3D.UnitX));
            LookUp = new DelegateCommand(() => Look(MathUtils3D.UnitX, +1));
            LookDown = new DelegateCommand(() => Look(MathUtils3D.UnitX, -1));
            LookLeft = new DelegateCommand(() => Look(MathUtils3D.UnitY, +1));
            LookRight = new DelegateCommand(() => Look(MathUtils3D.UnitY, -1));
            Primitives = new ReadOnlyObservableCollection<NewPrimitive>(sessionData.NewPrimitives);
            SnappedPrimitives = new SnappedPrimitivesCollection(sessionData.SnappedPrimitives);

            eventAggregator.GetEvent<SnapCompleteEvent>().Subscribe(OnSnapComplete);
        }

        public ICommand MoveForward { get; private set; }
        public ICommand MoveBack { get; private set; }
        public ICommand MoveLeft { get; private set; }
        public ICommand MoveRight { get; private set; }

        public ICommand LookUp { get; private set; }
        public ICommand LookDown { get; private set; }
        public ICommand LookLeft { get; private set; }
        public ICommand LookRight { get; private set; }

        public ReadOnlyObservableCollection<NewPrimitive> Primitives { get; private set; }
        public ReadOnlyObservableCollection<SnappedPrimitive> SnappedPrimitives { get; private set; }

        #region IsFlightMode property

        private bool isFlightMode;

        public bool IsFlightMode
        {
            get { return isFlightMode; }
            set
            {
                isFlightMode = value;
                RaisePropertyChanged(() => IsFlightMode);
            }
        }

        #endregion

        #region IsTrackBallMode property

        private bool isTrackBallMode;

        public bool IsTrackBallMode
        {
            get { return isTrackBallMode; }
            set
            {
                isTrackBallMode = value;
                RaisePropertyChanged(() => IsTrackBallMode);
                ResetCamera();
            }
        }

        #endregion

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

        #region RightDirection property

        public Vector3D RightDirection
        {
            get { return Vector3D.CrossProduct(LookDirection, UpDirection); }
        }

        #endregion

        public void TrackballTrack(double trackX, double trackY)
        {
            if (!IsTrackBallMode)
                return;

            var xDegrees = trackX * TRACKBALL_SPEED;
            var yDegrees = trackY * TRACKBALL_SPEED;

            var tempRightDirection = Vector3D.CrossProduct(LookDirection, UpDirection).Normalized();
            var tempUpDirection = UpDirection;
            var tempPosition = (Vector3D)Position;

            // perform rotation of xDegrees around "up" axis
            tempPosition = RotationHelper.RotateVector(tempPosition, tempUpDirection, xDegrees);
            tempRightDirection = RotationHelper.RotateVector(tempRightDirection, tempUpDirection, xDegrees);

            // perform rotation of yDegrees around left/right axis
            tempPosition = RotationHelper.RotateVector(tempPosition, tempRightDirection, yDegrees);
            tempUpDirection = RotationHelper.RotateVector(tempUpDirection, tempRightDirection, yDegrees);

            UpDirection = tempUpDirection;
            Position = (Point3D)tempPosition;
            LookDirection = -tempPosition.Normalized();
        }

        public void TrackBallZoom(int amount)
        {
            if (!IsTrackBallMode)
                return;

            var position = (Vector3D)Position;
            Position = (Point3D)(Math.Pow(1 + TRACKBALL_ZOOM_SPEED, amount) * position);
        }


        private void OnSnapComplete(object payload)
        {
            ((SnappedPrimitivesCollection)SnappedPrimitives).RaiseReset();
        }

        private void ResetCamera()
        {
            Position = DEFAULT_POSITION;
            LookDirection = DEFAULT_LOOK_DIRECTION;
            UpDirection = DEFAULT_UP_DIRECTION;
        }

        private void Move(Vector3D vec)
        {
            vec = GetWorldVector(ref vec);
            Position = Position + MOVE_SPEED * vec;
        }

        private void Look(Vector3D vec, double amount)
        {
            vec = GetWorldVector(ref vec);
            var rotation = new AxisAngleRotation3D(vec, amount * LOOK_SPEED);
            var transform = new RotateTransform3D { Rotation = rotation };

            lookDirection = transform.Transform(lookDirection);
            upDirection = transform.Transform(upDirection);

            RaisePropertyChanged(() => LookDirection);
            RaisePropertyChanged(() => UpDirection);
            RaisePropertyChanged(() => RightDirection);
        }

        private Vector3D GetWorldVector(ref Vector3D vector3D)
        {
            var xAxis = RightDirection.Normalized();
            var yAxis = upDirection.Normalized();
            var zAxis = lookDirection.Normalized();

            var moveVector = vector3D.X * xAxis + vector3D.Y * yAxis + vector3D.Z * zAxis;
            return moveVector;
        }

        public class SnappedPrimitivesCollection : ReadOnlyObservableCollection<SnappedPrimitive>
        {
            public SnappedPrimitivesCollection(ObservableCollection<SnappedPrimitive> list)
                : base(list)
            {
            }

            public void RaiseReset()
            {
                base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
