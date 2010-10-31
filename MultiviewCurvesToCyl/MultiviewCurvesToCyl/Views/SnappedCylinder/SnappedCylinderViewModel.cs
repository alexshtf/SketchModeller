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
using MultiviewCurvesToCyl.Base;

namespace MultiviewCurvesToCyl
{
    class SnappedCylinderViewModel : Base3DViewModel
    {
        public const double EPSILON = 1E-5;
        public const double INITIAL_LENGTH = 5;
        public const double INITIAL_RADIUS = 5;
        public static readonly Vector3D INITIAL_ORIENTATION = MathUtils3D.UnitX;
        public static readonly Vector3D INITIAL_VIEW_DIRECTION = MathUtils3D.UnitY;
        public static readonly Point3D INITIAL_CENTER = new Point3D();

        private IHaveCameraInfo cameraInfo;
        private MeshTopologyInfo topologyInfo;
        private DispatcherTimer snappingTimer;

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
        public void Initialize(double radius, double length, Point3D center, Vector3D orientation, IHaveCameraInfo initCameraInfo, bool wireframe)
        {
            Contract.Requires(IsInitialized == false, "Cannot initialize the object twice");
            Contract.Requires(initCameraInfo != null);

            cameraInfo = initCameraInfo;
            CylinderData = new ConstrainedCylinder(radius, length, center, orientation, cameraInfo.ViewDirection);
            topologyInfo = new MeshTopologyInfo(CylinderData.TriangleIndices);
            IsInWireframeMode = wireframe;
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
            var topBottomIndices = CylinderData.TopFiberIndices.Concat(CylinderData.BottomFiberIndices).ToArray();

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

            // here we construct an array of indices of points that will be constrained.
            // later we specify the points themselves, and they must be specified in THE SAME ORDER
            var inflationConstrainedIndices = new List<int>();
            foreach (var singleCircleIndices in pointsOnCirclesCache)
                inflationConstrainedIndices.AddRange(singleCircleIndices.All);
            inflationConstrainedIndices.AddRange(topBottomIndices.Except(allMovedOnCircles));

            var inflation = new FibermeshInflation(CylinderData, inflationConstrainedIndices.ToArray());

            // perform the smoothing steps with dispatcher timer (to show animation to the user).
            const int COUNT = 10;
            const double STEP_SIZE = 0.2;
            snappingTimer = new DispatcherTimer();
            snappingTimer.Interval = TimeSpan.FromSeconds(0.01);
            int ticks = 1;
            snappingTimer.Tick += (sender, args) =>
                {
                    // find error vectors for top/bottom fibers using closest projections on the curves.
                    var errorVectors = GetErrorVectors(curves3d, topBottomIndices);

                    // show the new correspondances to the user
                    CorrespondancesToShow.Clear();
                    CorrespondancesToShow.AddRange(
                        from item in errorVectors
                        select Tuple.Create(
                            CylinderData.Positions[item.Index], 
                            CylinderData.Positions[item.Index] - item.ErrorVector));

                    // the positions that we now manually move and will constrain during the smooth step
                    var manuallyMovedPoints = new List<Point3D>();

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

                        var transformedPoints = Transform3DCircle(CylinderData, singleCircleIndices.All.ToArray(), i1, i2, e1, e2, STEP_SIZE);

                        // now we apply the transformation t to all points on a single circle
                        for (int i = 0; i < singleCircleIndices.All.Count; ++i)
                            manuallyMovedPoints.Add(transformedPoints[i]);
                    }

                    // move the vertices along the error vectors, in small steps of size STEP_SIZE.
                    var leftToUpdate = errorVectors.Where(x => !allMovedOnCircles.Contains(x.Index));
                    foreach (var item in leftToUpdate)
                        manuallyMovedPoints.Add(CylinderData.Positions[item.Index] - STEP_SIZE * item.ErrorVector);

                    // perform smoothing step to spread the change to the whole mesh
                    var manuallyMovedPointsArray = manuallyMovedPoints.ToArray(); 
                    for (int i = 0; i < 5; ++i)
                        inflation.SmoothStep(manuallyMovedPointsArray);                    

                    // notify the user about position/normal updates on the whole mesh
                    for(int i = 0; i < CylinderData.Positions.Count; ++i)
                    {
                        NotifyPositionUpdated(i);
                        NotifyNormalUpdated(i);
                    }
                    
                    // stop the timer when we did the above steps COUNT times.
                    ++ticks;
                    if (ticks > COUNT)
                    {
                        snappingTimer.Stop();
                        errorVectors = GetErrorVectors(curves3d, topBottomIndices);

                        // show the new correspondances to the user
                        CorrespondancesToShow.Clear();
                        CorrespondancesToShow.AddRange(
                            from item in errorVectors
                            select Tuple.Create(
                                CylinderData.Positions[item.Index],
                                CylinderData.Positions[item.Index] - item.ErrorVector));
                    }
                    System.Diagnostics.Debug.WriteLine(ticks);
                };
            snappingTimer.Start();
        }

        private IEnumerable<ErrorVectorData> GetErrorVectors(IEnumerable<IEnumerable<Point3D>> curves3d, IEnumerable<int> topBottomIndices)
        {
            var errorVectors =
                from index in topBottomIndices
                let position = CylinderData.Positions[index]
                let projections = curves3d.Select(curve => position.ProjectionOnCurve(curve))
                let closestProjection = projections.Minimizer(x => x.Distance).Position
                select new ErrorVectorData { Index = index, ErrorVector = position - closestProjection };
            errorVectors = errorVectors.ToArray(); // execute the query. we are going to modify the positions.
            return errorVectors;
        }

        private Point3D[] Transform3DCircle(ConstrainedCylinder cylinder, int[] circleIndices, int i1, int i2, Vector3D e1, Vector3D e2, double stepSize)
        {
            Contract.Requires(cylinder != null);
            Contract.Requires(circleIndices != null);
            Contract.Requires(Contract.ForAll(circleIndices, index => index < cylinder.Positions.Count));
            Contract.Requires(Contract.Exists(circleIndices, index => index == i1));
            Contract.Requires(Contract.Exists(circleIndices, index => index == i2));
            Contract.Requires(stepSize > 0);
            Contract.Ensures(Contract.Result<Point3D[]>().Length == circleIndices.Length);

            // current positions of the snapped vertices
            var p1 = cylinder.Positions[i1];
            var p2 = cylinder.Positions[i2];

            // the approximate circle center
            var c = MathUtils3D.Lerp(p1, p2, 0.5); 

            // new positions of the snapped vertices
            var q1 = p1 - stepSize * e1;
            var q2 = p2 - stepSize * e2;

            // approximate the new circle radius/center
            var nr = (q2 - q1).Length / 2;
            var nc = MathUtils3D.Lerp(q1, q2, 0.5);

            // approximate new positions on the circle
            var result = new Point3D[circleIndices.Length];
            for (int i = 0; i < circleIndices.Length; ++i)
            {
                var p = cylinder.Positions[circleIndices[i]];
                var v = (p - c).Normalized();
                result[i] = nc + nr * v;
            }

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

        #region ErrorVectorData struct

        private struct ErrorVectorData
        {
            public int Index;
            public Vector3D ErrorVector;
        }

        #endregion
    }
}
