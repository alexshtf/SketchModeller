using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Logging;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;

        public Snapper(SessionData sessionData, ILoggerFacade logger, IEventAggregator eventAggregator, IUnityContainer container)
        {
            this.sessionData = sessionData;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
        }

        public void Snap()
        {
            var selectedPolylines =
                (from polyline in sessionData.SketchObjects.OfType<Polyline>()
                 where polyline.IsSelected == true
                 select polyline
                ).ToArray();

            var selectedPolygons =
                (from polygon in sessionData.SketchObjects.OfType<Polygon>()
                 where polygon.IsSelected == true
                 select polygon
                ).ToArray();

            var selectedCylinder =
                (from cylinder in sessionData.NewPrimitives.OfType<NewCylinder>()
                 where cylinder.IsSelected == true
                 select cylinder
                ).FirstOrDefault();

            // TODO: Find selected primitives of other kinds

            if (selectedCylinder != null)
            {
                var snappedCylinder = SnapCylinder(selectedPolylines, selectedPolygons, selectedCylinder);
                sessionData.SnappedPrimitives.Add(snappedCylinder);
                sessionData.NewPrimitives.Remove(selectedCylinder);
            }
            OptimizeAll();

            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        private void OptimizeAll()
        {
            var variables = new List<Variable>();
            var startVectorWriter = new VectorsWriter();

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                AddVecVars(variables, snappedCylinder.Axis);
                startVectorWriter.Write(snappedCylinder.AxisResult);

                AddVecVars(variables, snappedCylinder.AxisNormal);
                startVectorWriter.Write(snappedCylinder.AxisNormalResult);

                AddVecVars(variables, snappedCylinder.BottomCenter);
                startVectorWriter.Write(snappedCylinder.BottomCenterResult);

                variables.Add(snappedCylinder.Length);
                startVectorWriter.Write(snappedCylinder.LengthResult);

                variables.Add(snappedCylinder.Radius);
                startVectorWriter.Write(snappedCylinder.RadiusResult);
            }

            var dataTerms = sessionData.SnappedPrimitives.Select(p => p.DataTerm);
            var annotationTerms = sessionData.Annotations.Select(x => GetAnnotationTerm(x));
            var finalTerm = TermBuilder.Sum(dataTerms.Concat(annotationTerms).Append(0));

            var minimizer = Optimizer.Minimize(finalTerm, variables.ToArray(), startVectorWriter.ToArray());
            var resultReader = new VectorsReader(minimizer);

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                snappedCylinder.AxisResult = resultReader.ReadVector3D();
                snappedCylinder.AxisNormalResult = resultReader.ReadVector3D();
                snappedCylinder.BottomCenterResult = resultReader.ReadPoint3D();
                snappedCylinder.LengthResult = resultReader.ReadValue();
                snappedCylinder.RadiusResult = resultReader.ReadValue();
            }

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                snappedCylinder.BottomCircle = CirclePoints(
                    snappedCylinder.BottomCenterResult,
                    snappedCylinder.AxisNormalResult,
                    Vector3D.CrossProduct(snappedCylinder.AxisNormalResult, snappedCylinder.AxisResult),
                    snappedCylinder.RadiusResult,
                    20);

                snappedCylinder.TopCircle = CirclePoints(
                    snappedCylinder.BottomCenterResult + snappedCylinder.LengthResult * snappedCylinder.AxisResult, // top = bottom + l * axis
                    snappedCylinder.AxisNormalResult,
                    Vector3D.CrossProduct(snappedCylinder.AxisNormalResult, snappedCylinder.AxisResult),
                    snappedCylinder.RadiusResult,
                    20);
            }
        }

        private Term GetAnnotationTerm(Annotation x)
        {
            // TODO: Implement annotation terms
            return 0;
        }

        private static void AddVecVars(List<Variable> variables, TVec vec)
        {
            variables.Add((Variable)vec.X);
            variables.Add((Variable)vec.Y);
            variables.Add((Variable)vec.Z);
        }

        

        private Term ProjectionConstraint(TVec[] curveVars, PointsSequence projCurve)
        {
            var samples = CurveSampler.UniformSample(projCurve, curveVars.Length);
            var sampleTerms =
                from sample in samples
                select new TVec(sample.X, -sample.Y);

            var curveProj =
                from pnt in curveVars
                select new TVec(pnt[0], pnt[1]); // take only X and Y as projection operator

            return GeometricTests.DiffSquared(sampleTerms.ToArray(), curveProj.ToArray());
        }

        private Variable[] GetVars(IEnumerable<TVec> vectors)
        {
            return 
                vectors
                .SelectMany(vec => vec.GetTerms())
                .Cast<Variable>()
                .ToArray();
        }

        private TVec[] GenerateVarVectors(int count)
        {
            var result = new TVec[count];
            for (int i = 0; i < count; ++i)
                result[i] = GenerateVarVector();

            return result;
        }

        private static TVec GenerateVarVector()
        {
            return new TVec(new Variable(), new Variable(), new Variable());
        }

        private Dictionary<PointsSequence, CurveCategory> Categorize(IEnumerable<PointsSequence> sequences, params CurveCategory[] categories)
        {
            // create viewmode/view for the categorizer window
            var categorizerVM = container.Resolve<CategorizerViewModel>();
            var categorizerView = container.Resolve<CategorizerView>(new DependencyOverride<CategorizerViewModel>(categorizerVM));

            // display the categorizer window with the correct data
            categorizerVM.Setup(sequences, categories);
            categorizerView.ShowDialog();

            // extract data from the categorizer
            if (categorizerVM.IsFinished)
                return categorizerVM.Result;
            else
                return null;
        }

        public class CurveCategory
        {
            public CurveCategory(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }
    }
}
