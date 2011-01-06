using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;
using System.Diagnostics;
using System.Diagnostics.Contracts;

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

                var wpfPoints = new List<System.Windows.Point>();
                wpfPoints.Add(figure.StartPoint);

                var segment = figure.Segments.Cast<PolyLineSegment>().First();
                wpfPoints.AddRange(segment.Points);

                var modelPoints = from pnt in wpfPoints
                                  select new Point { X = pnt.X, Y = pnt.Y };

                if (figure.IsClosed)
                    polygons.Add(new Polygon(modelPoints));
                else
                    polylines.Add(new Polyline(modelPoints));
            }

            var result = new VectorImageData
            {
                Points = new Point[0],
                PolyLines = polylines.ToArray(),
                Polygons = polygons.ToArray(),
            };
            Normalize(result);

            return result;
        }

        private static void Normalize(VectorImageData vectorImageData)
        {
            var sequencesUnflattened = new List<IEnumerable<PointsSequence>>();
            sequencesUnflattened.Add(vectorImageData.Polygons ?? Enumerable.Empty<PointsSequence>());
            sequencesUnflattened.Add(vectorImageData.PolyLines ?? Enumerable.Empty<PointsSequence>());

            var points = sequencesUnflattened
                .SelectMany(sequences => sequences)
                .SelectMany(sequence => sequence.Points)
                .ToList();

            points.AddRange(vectorImageData.Points ?? Enumerable.Empty<Point>());

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

            foreach (var point in points)
            {
                point.X = point.X - tx;
                point.Y = point.Y - ty;

                if (sx > 0)
                    point.X = point.X / sx;
                if (sy > 0)
                    point.Y = point.Y / sy;
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
            var paths = g.Elements(pathName).ToArray();
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
            public Point[] Points { get; set; }
            public Polygon[] Polygons { get; set; }
            public Polyline[] PolyLines { get; set; }
        }
    }
}
