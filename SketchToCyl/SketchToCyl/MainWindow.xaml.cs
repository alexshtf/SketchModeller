using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Utils;
using System.Collections.Specialized;
using System.Windows.Media.Media3D;

namespace SketchToCyl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int HOVER_DISTANCE_THRESHOLD = 20;

        private MainWindowViewModel viewModel;
        private bool isStroking;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            viewModel.PromptSaveCurves += new EventHandler<PromptFileEventArgs>(PromptSaveToCSV);
            viewModel.PromptLoadCurves += new EventHandler<PromptFileEventArgs>(PromptLoadCurves);
            viewModel.Meshes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnMeshesCollectionChanged);
            DataContext = viewModel;
        }

        void OnMeshesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var modelPrototype = TryFindResource("BaseGeometryModel") as GeometryModel3D;
                if (modelPrototype != null)
                {
                    var newItems = e.NewItems.OfType<Tuple<Point3D[], Vector3D[], int[]>>();
                    foreach (var meshData in newItems)
                    {
                        var positions = meshData.Item1;
                        var normals = meshData.Item2;
                        var indices = meshData.Item3;

                        var model = modelPrototype.Clone();
                        model.Geometry = new MeshGeometry3D
                        {
                            Positions = new Point3DCollection(positions),
                            Normals = new Vector3DCollection(normals),
                            TriangleIndices = new Int32Collection(indices),
                        };
                        viewport3d.Children.Add(new ModelVisual3D { Content = model });
                    }
                }
            }
            else
                throw new NotSupportedException();
        }

        void PromptLoadCurves(object sender, PromptFileEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "csv";
            openDialog.AddExtension = true;
            openDialog.CheckFileExists = true;
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value == true)
                e.FileName = openDialog.FileName;
        }

        private void PromptSaveToCSV(object sender, PromptFileEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "csv";
            saveDialog.AddExtension = true;
            var dialogResult = saveDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value == true)
                e.FileName = saveDialog.FileName;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var inputElement = sender as IInputElement;
            var pnt = e.GetPosition(inputElement);
            if (isStroking)
                viewModel.AddPointToActiveStroke(pnt);
            else
            {
                // find the closest stroke and the projected point on the stroke.
                var projected = new Point();
                var projDist = double.MaxValue;
                var projIndex = int.MinValue;
                var projStrokeIndex = int.MinValue;
                foreach (var stroke in viewModel.Strokes.ZipIndex())
                {
                    var projResult = pnt.ProjectionOnCurve(stroke.Value);
                    if (projResult.Item2 < projDist)
                    {
                        projected = projResult.Item1;
                        projDist = projResult.Item2;
                        projIndex = projResult.Item3;
                        projStrokeIndex = stroke.Index;
                    }
                }

                if (projDist < HOVER_DISTANCE_THRESHOLD)
                {
                    viewModel.OnUpdateCloseStrokeInfo(projected, projDist, projIndex, projStrokeIndex);

                    pointSelection.X1 = pnt.X;
                    pointSelection.Y1 = pnt.Y;
                    pointSelection.X2 = projected.X;
                    pointSelection.Y2 = projected.Y;
                }
                else
                    pointSelection.X1 = pointSelection.Y2 = pointSelection.X2 = pointSelection.Y2 = 0;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isStroking = true;

                var inputElement = sender as IInputElement;
                inputElement.CaptureMouse();
                viewModel.AddPointToActiveStroke(e.GetPosition(inputElement));
                pointSelection.Visibility = Visibility.Hidden;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isStroking = false;
                viewModel.AddStroke();
                (sender as IInputElement).ReleaseMouseCapture();
                pointSelection.Visibility = Visibility.Visible;
            }
        }
    }
}
