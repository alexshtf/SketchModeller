using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using System.IO;
using Microsoft.Research.Science.Data;
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.Sketch
{
    class SketchCatalog : ISketchCatalog
    {
        private const string CATALOG_PATH = @"Sketches";
        private static readonly string CATALOG_FILE = Path.Combine(CATALOG_PATH, "catalog.xml");

        private ISketchProcessing sketchProcessing;
        
        [InjectionConstructor]
        public SketchCatalog(ISketchProcessing sketchProcessing)
        {
            this.sketchProcessing = sketchProcessing;
        }

        public IObservable<string[]> GetSketchNamesAsync()
        {
            return
                from sketchInfos in LoadCatalogAsync()
                let names = from info in sketchInfos
                            select info.Name
                select names.ToArray();
        }

        public IObservable<SketchData> LoadSketchAsync(string sketchName)
        {
            var result = from info in GetSketchMetadataAsync(sketchName)
                         let fileName = Path.Combine(CATALOG_PATH, info.File)
                         from image in sketchProcessing.LoadSketchImageAsync(fileName)
                         let infoFile = Path.Combine(CATALOG_PATH, info.InfoFile)
                         from data in LoadSketchDataAsync(infoFile)
                         select new SketchData 
                         { 
                             Image = image,
                             Points = data.Points,
                         };

            return result;
        }

        private IObservable<InternalSketchData> LoadSketchDataAsync(string infoFile)
        {
            Func<InternalSketchData> loadSketchData = () =>
                {
                    var infoElement = XElement.Load(infoFile);
                    var pointsFile = (string)infoElement.Element("points");
                    
                    var infoDirectory = Path.GetDirectoryName(Path.GetFullPath(infoFile));
                    pointsFile = Path.Combine(infoDirectory, pointsFile);
                    using (var csvPoints = DataSet.Open(pointsFile))
                    {
                        Trace.Assert(csvPoints.Dimensions.Count == 2, "CSV file must have two columns");
                        var xs = csvPoints[0];
                        var ys = csvPoints[1];
                        
                        Trace.Assert(xs.Rank == 1 && ys.Rank == 1, "Both columns must have rank 1");
                        Trace.Assert(xs.Dimensions[0].Length == ys.Dimensions[0].Length, "Both columns must be of the same length");

                        var xsArray = (double[])xs.GetData();
                        var ysArray = (double[])ys.GetData();
                        var pointsArray = new Point[xsArray.Length];
                        for (int i = 0; i < xsArray.Length; ++i)
                            pointsArray[i] = new Point { X = xsArray[i], Y = ysArray[i] };

                        return new InternalSketchData { Points = pointsArray };
                    }
                };
            return Observable.FromAsyncPattern<InternalSketchData>(
                loadSketchData.BeginInvoke, 
                loadSketchData.EndInvoke)();
        }

        private IObservable<SketchMetadata> GetSketchMetadataAsync(string sketchName)
        {
            return from sketchMetadatas in LoadCatalogAsync()
                   let foundInfo = (from item in sketchMetadatas
                                    where item.Name == sketchName
                                    select item).First()
                   select foundInfo;
        }

        private IObservable<SketchMetadata[]> LoadCatalogAsync()
        {
            Func<SketchMetadata[]> loadCatalog = () =>
            {
                var rootElement = XElement.Load(CATALOG_FILE);
                var metadata = from el in rootElement.Elements("sketch")
                               select new SketchMetadata
                               {
                                   Name = (string)el.Attribute("name"),
                                   File = (string)el.Attribute("file"),
                                   InfoFile = (string)el.Attribute("info"),
                               };
                return metadata.ToArray();
            };

            return Observable.FromAsyncPattern<SketchMetadata[]>(
                loadCatalog.BeginInvoke,
                loadCatalog.EndInvoke)();
        }

        private class SketchMetadata
        {
            public string Name { get; set; }
            public string File { get; set; }
            public string InfoFile { get; set; }
        }

        private class InternalSketchData
        {
            public Point[] Points { get; set; }
        }
    }
}
