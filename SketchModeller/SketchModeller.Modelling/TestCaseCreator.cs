using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using SketchModeller.Modelling.Events;
using System.Diagnostics;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using Utils;
using SketchModeller.Utilities;
using System.Windows.Media.Media3D;

using Enumerable = System.Linq.Enumerable;
using System.Windows;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling
{
    class TestCaseCreator
    {
        private readonly SessionData sessionData;
        private readonly UiState uiState;
        private readonly ISnapper snapper;

        [InjectionConstructor]
        public TestCaseCreator(IEventAggregator eventAggregator, SessionData sessionData, UiState uiState, ISnapper snapper)
        {
            this.sessionData = sessionData;
            this.uiState = uiState;
            this.snapper = snapper;
            eventAggregator.GetEvent<TestCaseEvent>().Subscribe(OnTestCase);
        }

        private void OnTestCase(object ignore)
        {
            // data from which the cylinder and its curve representations will be created
            var axis = RotationHelper.RotateVector(MathUtils3D.UnitY, MathUtils3D.UnitX, -20);
            var center = MathUtils3D.Origin;
            var radius = 0.2;
            var halfLength = 0.3;

            // basis vectors for the cylinder top/bottom planes
            var xBase = MathUtils3D.NormalVector(axis);
            var yBase = Vector3D.CrossProduct(xBase, axis);

            // generators for the top/bottom circles.
            var topGenerator = new PolarPointsGenerator(center + halfLength * axis, xBase, yBase);
            var bottomGenerator = new PolarPointsGenerator(center - halfLength * axis, xBase, yBase);

            // angles to use for circles generation.
            const int COUNT = 50;
            var angles =
                from i in Enumerable.Range(0, COUNT)
                select 2 * Math.PI * i / COUNT;

            // generate top/bottom circles in 3D
            var topCircle3d =
                from angle in angles
                select topGenerator.GetPoint(angle, radius);
            var bottomCircle3d =
                from angle in angles
                select bottomGenerator.GetPoint(angle, radius);

            // generate the left line from top to bottom
            var leftLine3d =
                from i in Enumerable.Range(0, COUNT)
                let top = topGenerator.GetPoint(0, radius)
                let bottom = bottomGenerator.GetPoint(0, radius)
                let fraction = i / (double)(COUNT - 1)
                select MathUtils3D.Lerp(top, bottom, fraction);

            // generate right line from top to bottom
            var rightLine3d =
                from i in Enumerable.Range(0, COUNT)
                let top = topGenerator.GetPoint(Math.PI, radius)
                let bottom = bottomGenerator.GetPoint(Math.PI, radius)
                let fraction = i / (double)(COUNT - 1)
                select MathUtils3D.Lerp(top, bottom, fraction);

            // project all lines to 2D
            var topCircle2d = Project(topCircle3d);
            var bottomCircle2d = Project(bottomCircle3d);
            var leftLine2d = Project(leftLine3d);
            var rightLine2d = Project(rightLine3d);

            // generate curve data
            var topCurve = new Polygon { CurveCategory = CurveCategories.Feature, Points = topCircle2d.ToArray() };
            var bottomCurve = new Polygon { CurveCategory = CurveCategories.Feature, Points = bottomCircle2d.ToArray() };
            var leftCurve = new Polyline { CurveCategory = CurveCategories.Silhouette, Points = leftLine2d.ToArray() };
            var rightCurve = new Polyline { CurveCategory = CurveCategories.Silhouette, Points = rightLine2d.ToArray() };

            // generate new primitive that exactly matches the curves
            var newcylinder = new NewCylinder { Axis = axis, Center = center, Length = halfLength * 2, Diameter = radius * 2 };

            // reset session data
            var sketchData =
                new SketchData
                {
                    NewPrimitives = new NewPrimitive[] { newcylinder },
                    Polygons = new Polygon[] { topCurve, bottomCurve },
                    Polylines = new Polyline[] { leftCurve, rightCurve },
                    SnappedPrimitives = new SnappedPrimitive[0],
                    Annotations = new Annotation[0],
                };
            sessionData.SketchData = sketchData;
            sessionData.Annotations.Clear();
            sessionData.Annotations.AddRange(sketchData.Annotations);
            sessionData.NewPrimitives.Clear();
            sessionData.NewPrimitives.AddRange(sketchData.NewPrimitives);
            sessionData.SketchObjects = sketchData.Polygons.Cast<PointsSequence>().Concat(sketchData.Polylines).ToArray();
            sessionData.SnappedPrimitives.Clear();
            sessionData.SnappedPrimitives.AddRange(sessionData.SnappedPrimitives);
        }

        private static IEnumerable<Point> Project(IEnumerable<Point3D> toProject)
        {
            return toProject.Select(x => new Point(x.X, -x.Y));
        }
    }
}
