using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Utils;
using MultiviewCurvesToCyl.MeshGeneration;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Collections.ObjectModel;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderViewModel : BaseEditorObjectViewModel
    {
        public const double INITIAL_LENGTH = 5;
        public const double INITIAL_RADIUS = 5;
        public static readonly Vector3D INITIAL_ORIENTATION = MathUtils3D.UnitX;
        public static readonly Vector3D INITIAL_VIEW_DIRECTION = MathUtils3D.UnitY;
        public static readonly Point3D INITIAL_CENTER = new Point3D();

        private IHaveCameraInfo cameraInfo;
        private MeshTopologyInfo topologyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnappedCylinderViewModel"/> class.
        /// </summary>
        public SnappedCylinderViewModel()
        {
            // we perform initial initialization such that in designer mode we will be able to create new instances of the
            // view model without needing special initialization.
            CylinderData = new ConstrainedCylinder(
                INITIAL_RADIUS, 
                INITIAL_LENGTH, 
                INITIAL_CENTER, 
                INITIAL_ORIENTATION, 
                INITIAL_VIEW_DIRECTION);
        }

        public ConstrainedCylinder CylinderData { get; private set;}

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get ; private set; }

        /// <summary>
        /// Initializes the cylinder data
        /// </summary>
        /// <param name="radius">The cylinder radius.</param>
        /// <param name="length">The cylinder length.</param>
        /// <param name="center">The cylinder center.</param>
        /// <param name="orientation">The cylinder orientation.</param>
        /// <param name="initCameraInfo">The camera info</param>
        public void Initialize(double radius, double length, Point3D center, Vector3D orientation, IHaveCameraInfo initCameraInfo)
        {
            Contract.Requires(IsInitialized == false, "Cannot initialize the object twice");
            Contract.Requires(initCameraInfo != null);

            cameraInfo = initCameraInfo;
            CylinderData = new ConstrainedCylinder(radius, length, center, orientation, cameraInfo.ViewDirection);
            topologyInfo = new MeshTopologyInfo(CylinderData.TriangleIndices);
            IsInitialized = true;
        }

        /// <summary>
        /// Snaps the cylinder to the specified curves.
        /// </summary>
        /// <param name="curves">The curves.</param>
        public void SnapTo(IEnumerable<SketchCurve> curves)
        {
            Contract.Requires(curves != null);
            Contract.Requires(curves.Count() == 2);
            Contract.Requires(Contract.ForAll(curves, curve => curve.HasAnnotation<StartEndAnnotation>()));

            // get curve points between start/end points
            var curves3d =
                from curve in curves
                select GetCurvePoints(curve);
            curves3d = curves3d.ToArray();

            var nonCircleIndices = new HashSet<int>(CylinderData.ConstrainedPositionIndices);
            nonCircleIndices.ExceptWith(CylinderData.FirstCircleIndices);
            nonCircleIndices.ExceptWith(CylinderData.LastCircleIndices);

            // find correspondance 
            var correspondances =
                from index in nonCircleIndices
                let pnt = CylinderData.Positions[index]
                let proj = curves3d.Select(curve => pnt.ProjectionOnCurve(curve))
                let minProj = proj.Minimizer(x => x.Distance).Position
                select Tuple.Create(index, minProj);

            CorrespondancesToShow = new ObservableCollection<Tuple<Point3D, Point3D>>();
            CorrespondancesToShow.AddRange(
                from correspondanceTuple in correspondances
                select Tuple.Create(CylinderData.Positions[correspondanceTuple.Item1], correspondanceTuple.Item2));

            // perform the inflation process
            const int COUNT = 20;
            const double STEP_SIZE = 0.01;
            for (int i = 0; i < COUNT; ++i)
            {
                //// find error vectors
                //var errorVectors =
                //    from index in nonCircleIndices
                //    let pnt = CylinderData.Positions[index]
                //    let proj = curves3d.Select(curve => pnt.ProjectionOnCurve(curve))
                //    let minProj = proj.Minimizer(x => x.Distance).Position
                //    select new { Index = index, ErrorVector = pnt - minProj };

                //// move the vertices a bit to their new positions
                //foreach (var item in errorVectors)
                //{
                //    CylinderData.Positions[item.Index] = CylinderData.Positions[item.Index] - STEP_SIZE * item.ErrorVector;
                //    NotifyPositionUpdated(item.Index);
                //}

                // perform smoothing
                ConstrainedMeshSmooth.Step(CylinderData.Positions, CylinderData.Normals, topologyInfo, CylinderData.ConstrainedPositionIndices);
            }
        }

        public ObservableCollection<Tuple<Point3D, Point3D>> CorrespondancesToShow { get; private set; } 

        /// <summary>
        /// Occurs when a position of a vertex on the mesh is updated.
        /// </summary>
        public event EventHandler<IndexedAttributeUpdateEventArgs> PositionUpdated;

        /// <summary>
        /// Occurs when a normal of a vertex on the mesh is updated.
        /// </summary>
        public event EventHandler<IndexedAttributeUpdateEventArgs> NormalUpdated;

        private IEnumerable<Point3D> GetCurvePoints(SketchCurve curve)
        {
            var startEndAnnotation = curve.GetAnnotations<StartEndAnnotation>().First();

            var minIndex = Math.Min(startEndAnnotation.StartIndex, startEndAnnotation.EndIndex);
            var maxIndex = Math.Max(startEndAnnotation.StartIndex, startEndAnnotation.EndIndex);
            var slice = new ListSlice<Point>(curve.PolylinePoints, minIndex, maxIndex + 1); // maxIndex + 1 because ListSlice is exclusive of the end index.

            return
                from pnt in slice
                select new Point3D(pnt.X, pnt.Y, 0);
        }

        #region Update notification methods

        private void NotifyPositionUpdated(int index)
        {
            NotifyIndexUpdated(index, PositionUpdated);
        }

        private void NotifyNormalUpdated(int index)
        {
            NotifyIndexUpdated(index, NormalUpdated);
        }

        private void NotifyIndexUpdated(int index, EventHandler<IndexedAttributeUpdateEventArgs> method)
        {
            if (method != null)
                method(this, new IndexedAttributeUpdateEventArgs(index));
        }

        #endregion
    }
}
