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
using System.Diagnostics.Contracts;
using System.Windows.Media.Media3D;

namespace SkelToCyl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var skeleton = GenerateSkeleton();
            var mesh = SkeletonToMesh.SkeletonToCylinder(skeleton, 30);

            skeletonModel.Points = new Point3DCollection(
                from item in skeleton
                    select item.Position);

            modelMesh.Positions = new Point3DCollection(mesh.Item1);
            modelMesh.Normals = new Vector3DCollection(mesh.Item2);
            modelMesh.TriangleIndices = new Int32Collection(mesh.Item3);

            wireframe.Positions = modelMesh.Positions.Clone();
            wireframe.Normals = modelMesh.Normals.Clone();
            wireframe.TriangleIndices = modelMesh.TriangleIndices.Clone();
        }

        private IEnumerable<SkeletonPoint> GenerateSkeleton()
        {
            foreach (var t in LinSpace(0, 2 * Math.PI, 100))
            {
                yield return new SkeletonPoint
                {
                    Position = new Point3D(5 * Math.Cos(t), 0.5 * t, 5 * Math.Sin(t)),
                    Normal = new Vector3D(-5 * Math.Sin(t), 2, 5 * Math.Cos(t)),
                    Radius = 0.5 + 0.3 * Math.Cos(6 * t),
                };
            }
        }

        private IEnumerable<double> LinSpace(double min, double max, int nSteps)
        {
            Contract.Requires(nSteps >= 2);
            Contract.Requires(max > min);

            Contract.Ensures(Contract.Result<IEnumerable<double>>().Count() == nSteps);
            Contract.Ensures(Contract.Result<IEnumerable<double>>().First() == min);
            Contract.Ensures(Contract.Result<IEnumerable<double>>().Last() == max);

            return LinSpaceCore(min, max, nSteps);
        }

        private IEnumerable<double> LinSpaceCore(double min, double max, int nSteps)
        {
            for (int i = 0; i < nSteps; ++i)
            {
                if (i == 0)
                    yield return min;
                else if (i == nSteps - 1)
                    yield return max;
                else
                {
                    var current = min + i * (max - min) / (nSteps - 1);
                    yield return current;
                }
            }
        }
    }
}
