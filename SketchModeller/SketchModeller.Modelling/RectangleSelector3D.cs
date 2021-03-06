﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics.Contracts;
using System.IO;
using System.Diagnostics;

namespace SketchModeller.Modelling
{
    /// <summary>
    /// Allows selecting 3D objects using a rectangle. Uses a simple hack of rendering to image to perform
    /// this selection (as WPF does not support geometry hit testing in Viewport3D).
    /// </summary>
    public static class RectangleSelector3D
    {
        /// <summary>
        /// Selects objects that are visible under the given rectangle inside the given Viewport3D.
        /// </summary>
        /// <param name="vp3d">The Viewport3D where the 3D scene resides.</param>
        /// <param name="rect">The rectangle where to look for objects, in <paramref name="vp3d"/>'s local coordinates.</param>
        /// <param name="filter">A function that filters, by returning <c>false</c>, objects that we do not want to select. Used to improve
        /// performance by filtering un-needed pixels. <c>null</c> means all objects are selectable.</param>
        /// <returns>A collection of objects that are visible inside the rectangle defined by <paramref name="rect"/> in 
        /// the viewport that pass the criterion defined by <paramref name="filter"/></returns>
        public static IEnumerable<ModelVisual3D> Select(this Viewport3D vp3d, Rect rect, Predicate<ModelVisual3D> filter = null)
        {
            Contract.Requires(vp3d != null);
            Contract.Ensures(Contract.Result<IEnumerable<ModelVisual3D>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ModelVisual3D>>(), v => v != null));
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ModelVisual3D>>(), v => filter == null || filter(v) == true));

            if (filter == null)
                filter = _ => true;

            var pixelsWidth = (int)vp3d.ActualWidth;
            var pixelsHeight = (int)vp3d.ActualHeight;

            // duplicate viewport3d
            var dup = Duplicate(vp3d, filter);
            dup.Children.Insert(0, new ModelVisual3D
            {
                Content = new AmbientLight { Color = Colors.White },
            });
            dup.Width = pixelsWidth;
            dup.Height = pixelsHeight;

            // apply camera
            dup.Camera = vp3d.Camera.CloneCurrentValue();

            // force layout
            dup.Measure(new Size(dup.Width, dup.Height));
            dup.Arrange(new Rect(0, 0, dup.Width, dup.Height));

            // render to bitmap
            var rtBitmap = new RenderTargetBitmap(pixelsWidth, pixelsHeight, 96, 96, PixelFormats.Default);
            rtBitmap.Render(dup);
            //DebugSave(rtBitmap);

            // get the pixels
            int[] pixels = new int[pixelsWidth * pixelsHeight];
            rtBitmap.CopyPixels(pixels, pixelsWidth * 4, 0);
            const int SELECTABLE_COLOR = -16908288;

            // get coordinates of the pixels having the feature curves color
            var intRect = new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            var coordsQuery = from x in Enumerable.Range(intRect.X, intRect.Width).AsParallel()
                              from y in Enumerable.Range(intRect.Y, intRect.Height)
                              let i = x + (y * pixelsWidth)
                              where pixels[i] == SELECTABLE_COLOR
                              select new Point(x, y);
            var coords = coordsQuery.ToArray();
            //DebugSave(pixels, coords, pixelsWidth, pixelsHeight);

            // perform hit-testing in all the relevant pixels
            var resultsSet = new HashSet<ModelVisual3D>();
            foreach (var pnt in coords)
            {
                var htResult = PerformHitTest(vp3d, pnt, filter);
                if (htResult != null)
                    resultsSet.Add(htResult);
            }

            return resultsSet.ToArray();
        }

        #region DebugSave methods. Mostly unused, except for debugging

        //private static void DebugSave(int[] pixels, Point[] coords, int width, int height)
        //{
        //    WriteableBitmap bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
        //    bmp.Lock();
        //    bmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        //    bmp.Unlock();

        //    DrawingVisual visual = new DrawingVisual();
        //    var pen = new Pen(Brushes.Orange, 1);
        //    var ctx = visual.RenderOpen();
        //    foreach (var pnt in coords)
        //        ctx.DrawRectangle(Brushes.Yellow, pen, new Rect(pnt, new Size(1, 1)));
        //    ctx.DrawRectangle(null, new Pen(Brushes.Black, 2), new Rect(0, 0, width, height));
        //    ctx.Close();
        //    var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //    rtb.Render(visual);

        //    var canvas = new Canvas();
        //    canvas.Width = width;
        //    canvas.Height = height;
        //    canvas.Children.Add(new Image { Source = bmp, Width = width, Height = height });
        //    canvas.Children.Add(new Image { Source = rtb, Width = width, Height = height, Opacity = 0.7 });
        //    canvas.Measure(new Size(width, height));
        //    canvas.Arrange(new Rect(0, 0, width, height));

        //    rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //    rtb.Render(canvas);
        //    DebugSave(rtb);
        //}

        //private static void DebugSave(RenderTargetBitmap rtBitmap)
        //{
        //    var encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(rtBitmap));
        //    using (var stream = File.Create("bitmap.png"))
        //    {
        //        encoder.Save(stream);
        //    }
        //}

        #endregion

        private static ModelVisual3D PerformHitTest(Viewport3D vp3d, Point pnt, Predicate<ModelVisual3D> selector)
        {
            ModelVisual3D result = null;
            var htParams = new PointHitTestParameters(pnt);
            VisualTreeHelper.HitTest(vp3d,
                null,
                htResult =>
                {
                    var candidate = htResult.VisualHit as ModelVisual3D;
                    if (candidate != null && selector(candidate) == true)
                    {
                        result = candidate;
                        return HitTestResultBehavior.Stop;
                    }
                    else
                        return HitTestResultBehavior.Continue;
                },
                htParams);
            return result;
        }

        private static Viewport3D Duplicate(Viewport3D vp3d, Predicate<ModelVisual3D> selector)
        {
            var result = new Viewport3D();
            foreach (var child in vp3d.Children)
                result.Children.Add(Duplicate(child, selector));
            return result;
        }

        private static Visual3D Duplicate(Visual3D child, Predicate<ModelVisual3D> selector)
        {
            var modelVisual3d = child as ModelVisual3D;
            if (modelVisual3d != null)
                return Duplicate(modelVisual3d, selector);
            else
                throw new NotSupportedException("We do not support duplication of visuals of type " + child.GetType());
        }

        private static Visual3D Duplicate(ModelVisual3D modelVisual3d, Predicate<ModelVisual3D> selector)
        {
            bool hasBeenSelected = selector(modelVisual3d);
            var content = modelVisual3d.Content;
            if (content != null)
            {
                content = Duplicate(content, hasBeenSelected);
                if (content != null)
                    content.Freeze();
            }

            var result = new ModelVisual3D();
            result.Content = content;
            result.Transform = modelVisual3d.Transform.CloneCurrentValue();
            foreach (var child in modelVisual3d.Children)
                result.Children.Add(Duplicate(child, selector));

            return result;
        }

        private static Model3D Duplicate(Model3D content, bool hasBeenSelected)
        {
            var geometryModel3D = content as GeometryModel3D;
            if (geometryModel3D != null)
            {
                var clone = geometryModel3D.CloneCurrentValue();
                if (hasBeenSelected)
                    clone.Material = clone.BackMaterial = new DiffuseMaterial { Brush = Brushes.White, AmbientColor = Colors.Red };
                else
                    clone.Material = clone.BackMaterial = new DiffuseMaterial { Brush = Brushes.White, AmbientColor = Colors.Blue };
                return clone;
            }

            var light = content as Light;
            if (light != null)
                return null;

            var group = content as Model3DGroup;
            if (group != null)
            {
                var clone = new Model3DGroup();
                clone.Transform = group.Transform.CloneCurrentValue();

                foreach (var child in group.Children)
                {
                    var dupChild = Duplicate(child, hasBeenSelected);
                    if (dupChild != null)
                        clone.Children.Add(dupChild);
                }
            }

            throw new NotSupportedException("Not supported model type");
        }
    }
}
