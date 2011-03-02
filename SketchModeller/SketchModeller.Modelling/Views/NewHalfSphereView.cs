using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using Utils;
using SketchModeller.Utilities;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using SketchModeller.Infrastructure;
using Petzold.Media3D;

namespace SketchModeller.Modelling.Views
{
    class NewHalfSphereView : BaseNewPrimitiveView
    {
        private readonly NewHalfSphereViewModel viewModel;

        public NewHalfSphereView(NewHalfSphereViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;
        }

        public override void DragStart(Point startPos, LineRange startRay)
        {
            throw new NotImplementedException();
        }

        public override void Drag(Point currPos, LineRange currRay)
        {
            throw new NotImplementedException();
        }

        public override void DragEnd()
        {
            throw new NotImplementedException();
        }

        public override bool IsDragging
        {
            get { throw new NotImplementedException(); }
        }

        #region old code

        //protected override void MovePosition(Vector3D moveVector)
        //{
        //    viewModel.Center = viewModel.Center + moveVector;
        //}

        //protected override void Edit(int sign)
        //{
        //    viewModel.Edit(sign);
        //}

        //private void UpdateTranslation()
        //{
        //    UpdateTranslation(viewModel.Center);
        //}

        //private void UpdateHalfSphereAxis()
        //{
        //    var rotationAxis = Vector3D.CrossProduct(MathUtils3D.UnitY, viewModel.Axis);
        //    var degrees = Vector3D.AngleBetween(MathUtils3D.UnitY, viewModel.Axis);
        //    UpdateRotation(rotationAxis, degrees);
        //}

        //private void UpdateHalfSphereRadius()
        //{
        //    RegenerateGeometry();
        //}

        //private void UpdateHalfSphereLength()
        //{
        //    RegenerateGeometry();
        //}

        //private void RegenerateGeometry()
        //{
        //    const int RADIUS_SUBDIVISION = 50;
        //    const int LENGTH_SUBDIVISION = 20;

        //    // generate circles
        //    var circles = new List<Point3D[]>();
        //    for (int i = 0; i < LENGTH_SUBDIVISION; ++i)
        //    {
        //        var lsFraction = i / (double)LENGTH_SUBDIVISION;
        //        var lsAngle = 0.5 * Math.PI * lsFraction;
        //        var curRadius = viewModel.Radius * Math.Cos(lsAngle);
        //        var curCenter = MathUtils3D.Origin - lsFraction * viewModel.Length * viewModel.Axis;
        //        var circle = GenerateCircle(curCenter, curRadius, RADIUS_SUBDIVISION);
        //        circles.Add(circle);
        //    }

        //    // calculate the vertex (the last empty circle)
        //    var vertex = MathUtils3D.Origin - viewModel.Length * viewModel.Axis;

        //    var meshGeometry = new MeshGeometry3D();
        //    meshGeometry.Positions = new Point3DCollection(circles.Flatten().Append(vertex));
        //    meshGeometry.TriangleIndices = new Int32Collection();

        //    // generate triangle indices between circles
        //    for (var i = 0; i < circles.Count - 1; ++i)
        //    {
        //        int[] prevIdx = System.Linq.Enumerable.Range(RADIUS_SUBDIVISION * i, RADIUS_SUBDIVISION).ToArray();
        //        int[] nextIdx = System.Linq.Enumerable.Range(RADIUS_SUBDIVISION * (i + 1), RADIUS_SUBDIVISION).ToArray();
        //        for (var j = 0; j < RADIUS_SUBDIVISION; ++j)
        //        {
        //            var k = (j + 1) % RADIUS_SUBDIVISION;
        //            meshGeometry.TriangleIndices.AddMany(prevIdx[j], prevIdx[k], nextIdx[k]);
        //            meshGeometry.TriangleIndices.AddMany(prevIdx[j], nextIdx[k], nextIdx[j]);
        //        }
        //    }

        //    // generate triangles between last circle and vertex
        //    var lastIdx = System.Linq.Enumerable.Range(circles.Count * RADIUS_SUBDIVISION, RADIUS_SUBDIVISION).ToArray();
        //    var vertexIdx = meshGeometry.Positions.Count - 1;
        //    for (int j = 0; j < RADIUS_SUBDIVISION; ++j)
        //    {
        //        var k = (j + 1) % RADIUS_SUBDIVISION;
        //        meshGeometry.TriangleIndices.AddMany(lastIdx[j], lastIdx[k], vertexIdx);
        //    }

        //    meshGeometry.Freeze();
        //    UpdateGeometry(meshGeometry);
        //}

        //private Point3D[] GenerateCircle(Point3D center, double radius, int count)
        //{
        //    return ShapeHelper.GenerateCircle(center, MathUtils3D.UnitX, MathUtils3D.UnitZ, radius, count);
        //}

        //bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        //{
        //    if (managerType != typeof(PropertyChangedEventManager))
        //        return false;

        //    var eventArgs = (PropertyChangedEventArgs)e;

        //    eventArgs.Match(() => viewModel.Center, UpdateTranslation);
        //    eventArgs.Match(() => viewModel.Length, UpdateHalfSphereLength);
        //    eventArgs.Match(() => viewModel.Axis, UpdateHalfSphereAxis);
        //    eventArgs.Match(() => viewModel.Radius, UpdateHalfSphereRadius);

        //    return true;
        //}

        #endregion
    }
}
