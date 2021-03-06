﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using Utils;

namespace SketchModeller.Modelling.Services.Sketch
{
    partial class SketchCatalog
    {
        private VectorImageData LoadSvg(string fileName)
        {
            var polygons = new List<Polygon>();
            var polylines = new List<Polyline>();

            var figures = LoadFigures(fileName);
            foreach (var figure in figures)
            {
                Contract.Assume(figure.Segments.Cast<PolyLineSegment>().Count() == 1);

                var points = new List<Point>();
                points.Add(figure.StartPoint);

                var segment = figure.Segments.Cast<PolyLineSegment>().First();
                points.AddRange(segment.Points);

                if (figure.IsClosed)
                    polygons.Add(new Polygon(points));
                else
                    polylines.Add(new Polyline(points));
            }

            var result = new VectorImageData
            {
                PolyLines = polylines.ToArray(),
                Polygons = polygons.ToArray(),
            };
            Normalize(result);

            return result;
        }

        private static void Normalize(VectorImageData vectorImageData)
        {
            var sequencesUnflattened = new List<IEnumerable<PointsSequence>>();
            sequencesUnflattened.Add(vectorImageData.Polygons ?? System.Linq.Enumerable.Empty<PointsSequence>());
            sequencesUnflattened.Add(vectorImageData.PolyLines ?? System.Linq.Enumerable.Empty<PointsSequence>());
            
            var sequences = sequencesUnflattened.Flatten();
            var points = sequences.SelectMany(sequence => sequence.Points).ToList();

            var xs = points.Select(pnt => pnt.X).ToArray();
            var ys = points.Select(pnt => pnt.Y).ToArray();

            var minX = xs.Min();
            var maxX = xs.Max();
            var minY = ys.Min();
            var maxY = ys.Max();

            var tx = (maxX + minX) / 2;
            var sx = (maxX - minX) / 2;

            var ty = (maxY + minY) / 2;
            var sy = (maxY - minY) / 2;

            var scale = Math.Max(sx, sy);

            foreach (var seq in sequences)
            {
                foreach(var idx in System.Linq.Enumerable.Range(0, seq.Points.Length))
                {
                    // get the point
                    var point = seq.Points[idx];

                    // modify the point
                    point.X = point.X - tx;
                    point.Y = point.Y - ty;

                    if (sx > 0)
                        point.X = point.X / scale;
                    if (sy > 0)
                        point.Y = point.Y / scale;
                    
                    // put the modified point
                    seq.Points[idx] = point;
                }
            }
        }

        private static PathFigure[] LoadFigures(string svgFile)
        {
            var result = new List<PathFigure>();

            var document = XDocument.Load(svgFile);
            var xmlns = document.Root.GetDefaultNamespace().NamespaceName;
            var gName = XName.Get("g", xmlns);
            var pathName = XName.Get("path", xmlns);

            var g = document.Root.Element(gName);
            var paths = g != null ? g.Elements(pathName).ToArray() : document.Root.Elements(pathName).ToArray();
            foreach (var path in paths)
            {
                var data = (string)path.Attribute("d");

                var geometry = Geometry.Parse(data);
                var pathGeometry = geometry.GetFlattenedPathGeometry();
                Debug.Assert(pathGeometry.MayHaveCurves() == false);

                var transformText = (string)path.Attribute("transform");
                var transform = GetTransform(transformText);

                var figures = ConsolidateFigures(pathGeometry, transform);

                result.AddRange(figures);
                pathGeometry.Transform = transform;
            }

            return result.ToArray();
        }

        private static Transform GetTransform(string transformText)
        {
            if (!string.IsNullOrEmpty(transformText))
            {
                var transformSplit = transformText.Split('(', ')', ',');
                if (transformSplit[0] == "matrix")
                {
                    var transformValues =
                        (from str in transformSplit.Skip(1).Take(6)
                         select double.Parse(str)).ToArray();

                    return
                        new MatrixTransform(
                            transformValues[0], transformValues[1], transformValues[2], transformValues[3], transformValues[4], transformValues[5]);
                }
                if (transformSplit[0] == "translate")
                {
                    var dx = double.Parse(transformSplit[1]);
                    var dy = double.Parse(transformSplit[2]);
                    return new TranslateTransform(dx, dy);
                }
            }

            return null;
        }

        private static IEnumerable<PathFigure> ConsolidateFigures(PathGeometry pathGeometry, Transform transform)
        {
            var result = new List<PathFigure>();

            if (transform == null)
                transform = Transform.Identity;

            foreach (var figure in pathGeometry.Figures)
            {
                var points = new List<System.Windows.Point>();
                foreach (var segment in figure.Segments)
                {
                    var lineSegment = segment as LineSegment;
                    var polyLineSegment = segment as PolyLineSegment;

                    if (lineSegment != null)
                        points.Add(transform.Transform(lineSegment.Point));

                    if (polyLineSegment != null)
                    {
                        var transformedPoints = polyLineSegment.Points.Select(p => transform.Transform(p));
                        points.AddRange(transformedPoints);
                    }
                }

                var newFigure = new PathFigure
                {
                    StartPoint = transform.Transform(figure.StartPoint),
                    IsClosed = figure.IsClosed,
                };
                newFigure.Segments.Add(new PolyLineSegment(points, true));

                result.Add(newFigure);
            }

            return result;
        }

        private class VectorImageData
        {
            public Polygon[] Polygons { get; set; }
            public Polyline[] PolyLines { get; set; }
        }
    }
}
