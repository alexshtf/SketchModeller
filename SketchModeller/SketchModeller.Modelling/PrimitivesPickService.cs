using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;

namespace SketchModeller.Modelling
{
    class PrimitivesPickService
    {
        #region PrimitiveData property

        public static readonly DependencyProperty PrimitiveDataProperty =
            DependencyProperty.RegisterAttached("PrimitiveData", typeof(SelectablePrimitive), typeof(PrimitivesPickService));

        public static SelectablePrimitive GetPrimitiveData(Visual3D target)
        {
            return (SelectablePrimitive)target.GetValue(PrimitiveDataProperty);
        }

        public static void SetPrimitiveData(Visual3D target, SelectablePrimitive value)
        {
            target.SetValue(PrimitiveDataProperty, value);
        }

        #endregion

        public static SelectablePrimitive PickPrimitiveData(Viewport3D viewport, Point pnt)
        {
            var visual = PickPrimitiveVisual(viewport, pnt);
            return visual == null ? null : GetPrimitiveData(visual);
        }

        public static Visual3D PickPrimitiveVisual(Viewport3D viewport, Point pnt)
        {
            // perform query
            Visual3D newPrimitive = null;
            Visual3D snappedPrimitive = null;
            var htParameters = new PointHitTestParameters(pnt);
            VisualTreeHelper.HitTest(
                viewport,
                null,
                htResult => 
                {
                    var visualHit = htResult.VisualHit as Visual3D;
                    if (visualHit == null)
                        return HitTestResultBehavior.Continue;

                    var query = from visual in visualHit.VisualPathUp().OfType<Visual3D>()
                                let data = GetPrimitiveData(visual)
                                where data != null
                                select new { Visual = visual, Data = data };
                    var primitiveData = query.FirstOrDefault();
                    if (primitiveData == null)
                        return HitTestResultBehavior.Continue;

                    // we give new primitives a priority
                    if (primitiveData.Data is NewPrimitive && newPrimitive == null)
                    {
                        newPrimitive = primitiveData.Visual;
                        return HitTestResultBehavior.Stop;
                    }

                    if (primitiveData.Data is SnappedPrimitive && snappedPrimitive == null)
                        snappedPrimitive = primitiveData.Visual;
                    return HitTestResultBehavior.Continue;
                },
                htParameters);

            // return a new primitive if found, because they have a priority
            if (newPrimitive != null)
                return newPrimitive;
            else
                return snappedPrimitive;
        }
    }
}

