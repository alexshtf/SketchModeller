using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;
using MultiviewCurvesToCyl.Base;
using System.Windows.Input;

namespace MultiviewCurvesToCyl
{
    partial class MainViewModel : BaseViewModel, IHaveCameraInfo
    {
        private double MIN_CURVE_LENGTH = 20;

        private readonly Persistence.PersistenceService persistenceService = new Persistence.PersistenceService();

        private readonly Func<string> chooseOpenFile;
        private readonly Func<string> chooseSaveFile;
        private List<Point> underConstructionCurve;
        private string lastSavedFile;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(underConstructionCurve != null);
            Contract.Invariant(
                Contract.ForAll(SketchCurvesViewModels, viewModel => CurveLength(viewModel.Curve.PolylinePoints) >= MIN_CURVE_LENGTH));

            Contract.Invariant(ViewDirection.LengthSquared > 0);
            Contract.Invariant(UpDirection.LengthSquared > 0);
            Contract.Invariant(MathUtils3D.AreOrthogonal(ViewDirection, UpDirection));
        }

        /// <summary>
        /// Constructs a new instance of the view model for the main window.
        /// </summary>
        /// <param name="chooseOpenFile"></param>
        /// <param name="chooseSaveFile"></param>
        public MainViewModel(Func<string> chooseOpenFile, Func<string> chooseSaveFile)
        {
            Contract.Requires(chooseOpenFile != null);
            Contract.Requires(chooseSaveFile != null);

            this.chooseOpenFile = chooseOpenFile;
            this.chooseSaveFile = chooseSaveFile;

            SketchCurvesViewModels = new ObservableCollection<SketchCurveViewModel>();
            underConstructionCurve = new List<Point>();
            NewCylinderViewModels = new ObservableCollection<NewCylinderViewModel>();
            SnappedCylinderViewModels = new ObservableCollection<SnappedCylinderViewModel>();
            PointsBasedCylinderViewModels = new ObservableCollection<PointsBasedCylinderViewModel>();

            MenuItems = new ObservableCollection<BaseMenuViewModel>
            {
                new MenuCategoryItem("File")
                {
                    MenuCommandItem.Create("Clear", o => Clear(), keyGesture: new KeyGesture(Key.C, ModifierKeys.Control)),
                    MenuCommandItem.Create("Open ...", o => Open(), keyGesture: new KeyGesture(Key.O, ModifierKeys.Control)),
                    MenuCommandItem.Create("Save", o => Save(), keyGesture: new KeyGesture(Key.S, ModifierKeys.Control)),
                    MenuCommandItem.Create("Save as ...", o => SaveAs(), keyGesture: new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)),
                    MenuCommandItem.Create("Exit", o => Exit()),
                },
                new MenuCategoryItem("Edit")
                {
                    MenuCommandItem.Create("New cylinder", o => NewCylinder(), keyGesture: new KeyGesture(Key.C, ModifierKeys.Alt)),
                },
                new MenuCategoryItem("View")
                {
                    MenuCommandItem.Create("Enter navigation mode", o => EnterNavigationMode(), keyGesture: new KeyGesture(Key.F2)),
                    MenuCommandItem.Create("Toggle wireframe", o => ToggleWireframe(), keyGesture: new KeyGesture(Key.F3)),
                },
            };

            ViewPosition = new Point3D(0, 0, 700);
            ViewDirection = new Vector3D(0, 0, -1);
            UpDirection = new Vector3D(0, 1, 0);
        }

        #region Menu commands

        public void Clear()
        {
            Contract.Ensures(SketchCurvesViewModels.Count == 0);
            Contract.Ensures(underConstructionCurve.Count == 0);
            Contract.Ensures(lastSavedFile == null);

            underConstructionCurve = new List<Point>();
            NotifyUnderConstructionCurveChanged();

            SketchCurvesViewModels.Clear();
            lastSavedFile = null;
        }

        public void Save()
        {
            var fileName = lastSavedFile == null ? chooseSaveFile() : lastSavedFile;
            SaveCore(fileName);
        }

        public void SaveAs()
        {
            var fileName = chooseSaveFile();
            SaveCore(fileName);
        }

        public void Open()
        {
            var fileName = chooseOpenFile();
            if (fileName != null)
            {
                var state = persistenceService.Load(fileName);
                LoadPersistentState(state);
                lastSavedFile = fileName;
            }
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        public void NewCylinder()
        {
            var newCylinderViewModel = CreateNewCylinderViewModel();
            NewCylinderViewModels.Add(newCylinderViewModel);
        }

        public void EnterNavigationMode()
        {
            IsInNavigationMode = true;
        }

        private NewCylinderViewModel CreateNewCylinderViewModel()
        {
            var result = new NewCylinderViewModel();
            result.ContextMenu.Add(MenuCommandItem.Create("Snap fibermesh", o => SnapFibermeshCylinder(result)));
            result.ContextMenu.Add(MenuCommandItem.Create("Snap points-based cylinder", o => SnapPointsBasedCylinder(result)));
            return result;
        }

        private void ToggleWireframe()
        {
            IsWireframeShown = !IsWireframeShown;
            foreach (var item in SnappedCylinderViewModels)
                item.IsInWireframeMode = IsWireframeShown;
            foreach (var item in NewCylinderViewModels)
                item.IsInWireframeMode = IsWireframeShown;
        }

        #endregion

        #region New cylinder commands

        public void SnapFibermeshCylinder(NewCylinderViewModel toSnap)
        {
            NewCylinderViewModels.Remove(toSnap);

            var snappedCylinderViewModel = new SnappedCylinderViewModel();

            var cameraInfo = this as IHaveCameraInfo;
            snappedCylinderViewModel.Initialize(
                toSnap.Radius, 
                toSnap.Length, 
                toSnap.Center, 
                toSnap.Orientation,
                cameraInfo,
                IsWireframeShown);
            snappedCylinderViewModel.SnapTo(SketchCurvesViewModels.Select(x => x.Curve));

            SnappedCylinderViewModels.Add(snappedCylinderViewModel);
        }

        private void SnapPointsBasedCylinder(NewCylinderViewModel toSnap)
        {
            NewCylinderViewModels.Remove(toSnap);
            var pointsBasedCylinder = new PointsBasedCylinderViewModel();
            pointsBasedCylinder.Initialize(IsWireframeShown);
            pointsBasedCylinder.SnapTo(SketchCurvesViewModels.Select(x => x.Curve));

            PointsBasedCylinderViewModels.Add(pointsBasedCylinder);
        }

        #endregion

        #region Public properties and methods

        public IEnumerable<MenuCommandItem> GetAllMenuCommandItems()
        {
            var stack = new Stack<BaseMenuViewModel>();
            foreach (var item in MenuItems)
                stack.Push(item);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                Contract.Assume(item is MenuCommandItem || item is MenuCategoryItem);
                var categoryItem = item as MenuCategoryItem;
                if (categoryItem != null)
                {
                    foreach (var child in categoryItem.Children)
                        stack.Push(child);
                }
                else
                    yield return (MenuCommandItem)item;
            }
        }

        #region IsWireframeShown property

        private bool isWireframeShown;

        public bool IsWireframeShown
        {
            get { return isWireframeShown; }
            set
            {
                isWireframeShown = value;
                NotifyPropertyChanged(() => IsWireframeShown);
            }
        }

        #endregion

        #region ViewDirection property

        private Vector3D viewDirection;

        public Vector3D ViewDirection
        {
            get { return viewDirection; }
            set
            {
                viewDirection = value;
                NotifyPropertyChanged(() => ViewDirection);
            }
        }

        #endregion

        #region ViewPosition property

        private Point3D viewPosition;

        public Point3D ViewPosition
        {
            get { return viewPosition; }
            set
            {
                viewPosition = value;
                NotifyPropertyChanged(() => ViewPosition);
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
                NotifyPropertyChanged(() => UpDirection);
            }
        }

        #endregion

        #region TotalCameraMatrix property

        private Matrix3D totalCamerMatrix;

        public Matrix3D TotalCameraMatrix
        {
            get { return totalCamerMatrix; }
            set
            {
                totalCamerMatrix = value;
                NotifyPropertyChanged(() => TotalCameraMatrix);
            }
        }

        #endregion

        #region IsInNavigationMode property

        private bool isInNavigationMode;

        public bool IsInNavigationMode
        {
            get { return isInNavigationMode; }
            set
            {
                isInNavigationMode = value;
                NotifyPropertyChanged(() => IsInNavigationMode);
            }
        }

        #endregion

        public ObservableCollection<BaseMenuViewModel> MenuItems { get; private set; }

        /// <summary>
        /// A collection of view models for all the sketch curves.
        /// </summary>
        public ObservableCollection<SketchCurveViewModel> SketchCurvesViewModels { get; private set; }

        /// <summary>
        /// A collection of view models for all new (non-snapped) cylinders
        /// </summary>
        public ObservableCollection<NewCylinderViewModel> NewCylinderViewModels { get; private set; }

        /// <summary>
        /// A collection of view models for all snapped cylinders.
        /// </summary>
        public ObservableCollection<SnappedCylinderViewModel> SnappedCylinderViewModels { get; private set; }

        /// <summary>
        /// A collection of view-models for all points-based cylinders.
        /// </summary>
        public ObservableCollection<PointsBasedCylinderViewModel> PointsBasedCylinderViewModels { get; private set; }

        public ReadOnlyCollection<Point> UnderConstructionCurve
        {
            get { return underConstructionCurve.AsReadOnly(); }
        }

        public void AddUnderConstructionPoint(Point p)
        {
            underConstructionCurve.Add(p);
            NotifyUnderConstructionCurveChanged();
        }

        public void DiscardUnderConstructionCurve()
        {
            underConstructionCurve.Clear();
            NotifyUnderConstructionCurveChanged();
        }

        public void CommitUnderConstructionCurve()
        {
            if (underConstructionCurve.Count > 1 && CurveLength(underConstructionCurve) >= MIN_CURVE_LENGTH)
            {
                var curveViewModel = new SketchCurveViewModel(underConstructionCurve);
                curveViewModel.CurveContextMenu.Add(MenuCommandItem.Create("Delete curve", o => DeleteCurve(curveViewModel)));
                SketchCurvesViewModels.Add(new SketchCurveViewModel(underConstructionCurve));
            }

            DiscardUnderConstructionCurve();
        }

        public void DeleteCurve(SketchCurveViewModel curveViewModel)
        {
            SketchCurvesViewModels.Remove(curveViewModel);
        }

        #endregion

        #region Private methods

        [Pure]
        private static double CurveLength(IEnumerable<Point> underConstructionCurve)
        {
            Contract.Requires(underConstructionCurve != null);
            Contract.Requires(underConstructionCurve.Count() >= 2);

            Contract.Ensures(Contract.Result<double>() >= 0);

            var lengths = from segment in underConstructionCurve.SeqPairs()
                          let p1 = segment.Item1
                          let p2 = segment.Item2
                          select (p2 - p1).Length;

            return lengths.Sum();
        }

        private void NotifyUnderConstructionCurveChanged()
        {
            NotifyPropertyChanged(() => UnderConstructionCurve);
        }

        private void SaveCore(string fileName)
        {
            if (fileName != null) // we got a valid file name from the view
            {
                var state = GetPersistentState();
                persistenceService.Save(fileName, state);
                lastSavedFile = fileName;
            }
        }


        #endregion

        /// <summary>
        /// Rotates the camera with a new view and up direction, keeping its position.
        /// </summary>
        /// <param name="newViewDirection">The new camera view direction</param>
        /// <param name="newUpDirection">The new camera up direction</param>
        public void RotateCamera(Vector3D newViewDirection, Vector3D newUpDirection)
        {
            viewDirection = newViewDirection;
            upDirection = newUpDirection;

            NotifyPropertyChanged(() => ViewDirection);
            NotifyPropertyChanged(() => UpDirection);
        }
    }
     

}
