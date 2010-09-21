using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

using PCurve = MultiviewCurvesToCyl.Persistence.Curve;
using PState = MultiviewCurvesToCyl.Persistence.State;
using PDepthAnnotation = MultiviewCurvesToCyl.Persistence.DepthAnnotation;
using PStartEndAnnotation = MultiviewCurvesToCyl.Persistence.StartEndAnnotation;
using PBaseAnnotation = MultiviewCurvesToCyl.Persistence.BaseAnnotation;
using PPoint = MultiviewCurvesToCyl.Persistence.Point;
using System.Windows;

namespace MultiviewCurvesToCyl
{
    partial class MainViewModel
    {
        #region Persistent state extraction

        private PState GetPersistentState()
        {
            var curves = ExtractCurves();

            var state = new PState
            {
                Curves = curves.ToArray(),
            };
            return state;
        }

        private List<PCurve> ExtractCurves()
        {
            // gather all curves from curve view models
            var curves = new List<PCurve>();
            foreach (var curveViewModel in SketchCurvesViewModels)
            {
                // gather all annotations from a curve.
                var annotations = ExtractAnnotations(curveViewModel);
                var curve = new PCurve
                {
                    Points = (from pnt in curveViewModel.Curve.PolylinePoints
                              select new PPoint { X = pnt.X, Y = pnt.Y }).ToArray(),
                    Annotations = annotations.ToArray(),
                };
                curves.Add(curve);
            }
            return curves;
        }

        private static List<PBaseAnnotation> ExtractAnnotations(SketchCurveViewModel curveViewModel)
        {
            var annotations = new List<PBaseAnnotation>();
            foreach (var annotation in curveViewModel.Curve.Annotations)
            {
                annotation
                    .MatchClass<DepthAnnotation>(depthAnnotation =>
                    {
                        annotations.Add(new PDepthAnnotation
                        {
                            Depth = depthAnnotation.Depth,
                            Index = depthAnnotation.Index,
                        });
                    })
                    .DoWithClass<StartEndAnnotation>(startEndAnnotation =>
                    {
                        annotations.Add(new PStartEndAnnotation
                        {
                            StartIndex = startEndAnnotation.StartIndex,
                            EndIndex = startEndAnnotation.EndIndex,
                        });
                    })
                    .DoWithClass<object>(o => { throw new InvalidOperationException("Not supported annotation type"); });
            }
            return annotations;
        }

        #endregion

        #region Persistent state loading

        private void LoadPersistentState(PState state)
        {
            Clear();

            foreach (var curve in state.Curves)
            {
                var points = from pnt in curve.Points
                             select new Point(pnt.X, pnt.Y);

                var depthAnnotatios = from annotation in curve.Annotations.OfType<PDepthAnnotation>()
                                      select new DepthAnnotation
                                      {
                                          Index = annotation.Index,
                                          Depth = annotation.Depth,
                                      } as ICurveAnnotation;
                var startEndAnnotations = from annotation in curve.Annotations.OfType<PStartEndAnnotation>()
                                          select new StartEndAnnotation
                                          {
                                              StartIndex = annotation.StartIndex,
                                              EndIndex = annotation.EndIndex,
                                          };
                var allAnnotations = depthAnnotatios; // more annotation types will be added here!

                var curveViewModel = new SketchCurveViewModel(points);
                curveViewModel.SetStartEnd(startEndAnnotations.First().StartIndex, startEndAnnotations.First().EndIndex);
                foreach (var annotation in allAnnotations)
                    curveViewModel.Curve.Annotations.Add(annotation);

                SketchCurvesViewModels.Add(curveViewModel);
            }
        } 

        #endregion
    }
}
