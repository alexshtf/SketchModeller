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
using System.Windows.Threading;
using Meta.Numerics.Matrices;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderViewModel : BaseEditorObjectViewModel
    {
        public const double EPSILON = 1E-5;
        public const double INITIAL_LENGTH = 5;
        public const double INITIAL_RADIUS = 5;
        public static readonly Vector3D INITIAL_ORIENTATION = MathUtils3D.UnitX;
        public static readonly Vector3D INITIAL_VIEW_DIRECTION = MathUtils3D.UnitY;
        public static readonly Point3D INITIAL_CENTER = new Point3D();

        private IHaveCameraInfo cameraInfo;
        private MeshTopologyInfo topologyInfo;
        private DispatcherTimer smoothStepTimer;

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

            // initialize other data
            CorrespondancesToShow = new ObservableCollection<Tuple<Point3D, Point3D>>();
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

            // we will use the set that contains both top and bottom fiber indices multiple times
            // therefore we calculate it once here
            var topBottomIndices = new HashSet<int>(CylinderData.TopFiberIndices);
            topBottomIndices.UnionWith(CylinderData.BottomFiberIndices);

            // we need to know for each circle - which points will move as a part of the correction step
            // because later we wish to correct the whole circle according to the transformation applied
            // to those points
            var pointsOnCirclesCache =
                from indices in CollectionUtils.Enumerate(CylinderData.FirstCircleIndices, CylinderData.LastCircleIndices)
                select new 
                { 
                    All = indices, 
                    ErrorCorrected = indices.Intersect(topBottomIndices).ToArray(),
                };
            pointsOnCirclesCache = pointsOnCirclesCache.ToArray();

            // We also store all the moved indices. We need it in order to not move them more than once after
            // we apply the whole-circle transformation
            var allMovedOnCircles = pointsOnCirclesCache.SelectMany(x => x.ErrorCorrected).ToArray();

            // there should be two such points on every circle
            Contract.Assume(Contract.ForAll(pointsOnCirclesCache, indices => indices.ErrorCorrected.Length == 2));

            var inflation = new FibermeshInflation(CylinderData);

            // perform the smoothing steps with dispatcher timer (to show animation to the user).
            const int COUNT = 20;
            const double STEP_SIZE = 0.1;
            smoothStepTimer = new DispatcherTimer();
            smoothStepTimer.Interval = TimeSpan.FromSeconds(0.5);
            int ticks = 0;
            smoothStepTimer.Tick += (sender, args) =>
                {
                    // find error vectors for top/bottom fibers using closest projections on the curves.
                    var errorVectors =
                        from index in topBottomIndices
                        let position = CylinderData.Positions[index]
                        let projections = curves3d.Select(curve => position.ProjectionOnCurve(curve))
                        let closestProjection = projections.Minimizer(x => x.Distance).Position
                        select new { Index = index, ErrorVector = position - closestProjection };
                    errorVectors = errorVectors.ToArray(); // execute the query. we are going to modify the positions.

                    // show the new correspondances to the user
                    CorrespondancesToShow.Clear();
                    CorrespondancesToShow.AddRange(
                        from item in errorVectors
                        select Tuple.Create(
                            CylinderData.Positions[item.Index], 
                            CylinderData.Positions[item.Index] - item.ErrorVector));

                    // the positions that we now manually move and will constrain during the smooth step
                    var manuallyMovedPoints = new List<Tuple<int, Point3D>>();

                    // we will calculate the transformation that is applied to both circles, and we will
                    // re-apply this transformation to the whole circle instead of just two points on this circle.
                    foreach(var singleCircleIndices in pointsOnCirclesCache)
                    {
                        var i1 = singleCircleIndices.ErrorCorrected[0];
                        var i2 = singleCircleIndices.ErrorCorrected[1];

                        // here we assume that error vectors for those indices exist because they also belong to 
                        // top/bottom fibers.
                        var e1 = errorVectors.Where(item => item.Index == i1).First().ErrorVector;
                        var e2 = errorVectors.Where(item => item.Index == i2).First().ErrorVector;

                        // store original positions
                        var p1 = CylinderData.Positions[i1];
                        var p2 = CylinderData.Positions[i2];

                        // calculate positions after modification
                        var q1 = p1 - STEP_SIZE * e1;
                        var q2 = p2 - STEP_SIZE * e2;

                        // calculate the linear transformation
                        var t = GetTransform(p1, q1, p2, q2);

                        // we assume that t is indeed the transformation transforming p1 -> q1 and p2 -> q2
                        Contract.Assume((p1 * t - q1).LengthSquared < EPSILON);
                        Contract.Assume((p2 * t - q2).LengthSquared < EPSILON);

                        // now we apply the transformation t to all points on a single circle
                        foreach (var index in singleCircleIndices.All)
                            manuallyMovedPoints.Add(Tuple.Create(index, CylinderData.Positions[index] * t));
                    }

                    // move the vertices along the error vectors, in small steps of size STEP_SIZE.
                    var leftToUpdate = errorVectors.Where(x => !allMovedOnCircles.Contains(x.Index));
                    foreach (var item in leftToUpdate)
                        manuallyMovedPoints.Add(Tuple.Create(item.Index, CylinderData.Positions[item.Index] - STEP_SIZE * item.ErrorVector));

                    for (int i = 0; i < 5; ++i)
                        inflation.SmoothStep(manuallyMovedPoints);

                    // perform smoothing step to spread the change to the whole mesh
                    //ConstrainedMeshSmooth.Step(CylinderData.Positions, CylinderData.Normals, topologyInfo, CylinderData.ConstrainedPositionIndices);
                    

                    // notify the user about position/normal updates on the whole mesh
                    for(int i = 0; i < CylinderData.Positions.Count; ++i)
                    {
                        NotifyPositionUpdated(i);
                        NotifyNormalUpdated(i);
                    }
                    
                    // stop the timer when we did the above steps COUNT times.
                    ++ticks;
                    if (ticks > COUNT)
                        smoothStepTimer.Stop();
                    System.Diagnostics.Debug.WriteLine(ticks);
                };
            smoothStepTimer.Start();
        }

        private Matrix3D GetTransform(Point3D p1, Point3D q1, Point3D p2, Point3D q2)
        {
            Matrix m = new Matrix(new double[,]
            {
                { p1.X, p1.Y, p1.Z, 0   , 0   , 0   , 0   , 0   , 0   , 1, 0, 0},
                { 0   , 0   , 0   , p1.X, p1.Y, p1.Z, 0   , 0   , 0   , 0, 1, 0},
                { 0   , 0   , 0   , 0   , 0   , 0   , p1.X, p1.Y, p1.Z, 0, 0, 1},
                { p2.X, p2.Y, p2.Z, 0   , 0   , 0   , 0   , 0   , 0   , 1, 0, 0},
                { 0   , 0   , 0   , p2.X, p2.Y, p2.Z, 0   , 0   , 0   , 0, 1, 0},
                { 0   , 0   , 0   , 0   , 0   , 0   , p2.X, p2.Y, p2.Z, 0, 0, 1},
            });
            ColumnVector vec = new ColumnVector(new double[] { q1.X, q1.Y, q1.Z, q2.X, q2.Y, q2.Z });

            var s = LinearAlgebra.LeastNormSolution(m, vec);
            var result = new Matrix3D(
                s[0], s[3], s[6], 0,
                s[1], s[4], s[7], 0,
                s[2], s[5], s[8], 0,
                s[9], s[10], s[11], 1);

            return result;
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
