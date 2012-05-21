using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class NewStraightGenCylinder : NewCylindricalPrimitive
    {
        public NewStraightGenCylinder()
        {
            Components = new CylinderComponent[]
            {
                new CylinderComponent(1, 0),
                new CylinderComponent(1, 1),
            };
        }

        protected override double TopRadiusInternal
        {
            get { return Components.Last().Radius; }
        }

        protected override double BottomRadiusInternal
        {
            get { return Components.First().Radius; }
        }

        public CylinderComponent[] Components { get; set; }

        public override void UpdateCurvesGeometry()
        {
            base.UpdateCurvesGeometry();

            // get projected versions of top/bottom circles
            var top = Center.Value + 0.5 * Length.Value * Axis.Value;
            var topCircle3d = ShapeHelper.GenerateCircle(top, Axis, TopRadiusInternal, 20);
            var topCircle = ShapeHelper.ProjectCurve(topCircle3d);

            var bottom = Center.Value - 0.5 * Length.Value * Axis.Value;
            var bottomCircle3d = ShapeHelper.GenerateCircle(bottom, Axis, BottomRadiusInternal, 20);
            var bottomCircle = ShapeHelper.ProjectCurve(bottomCircle3d);

            // find the axis in projected coordinates
            var tb = ShapeHelper.ProjectCurve(top, bottom);
            var axis2d = tb[0] - tb[1];

            // find the 2 silhouette lines
            var perp = new Vector(axis2d.Y, -axis2d.X);
            perp.Normalize();
            var lt = tb[0] + TopRadiusInternal * perp;
            var lb = tb[1] + BottomRadiusInternal * perp;
            var rt = tb[0] - TopRadiusInternal * perp;
            var rb = tb[1] - BottomRadiusInternal * perp;

            var leftLine = new Point[] { lt, lb };
            var rightLine = new Point[] { rt, rb };
        }
    }
}
