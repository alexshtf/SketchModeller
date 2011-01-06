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
using System.Xml.Serialization;

namespace SketchModeller.Modelling.Services.Sketch
{
    partial class SketchCatalog : ISketchCatalog
    {
        private const string CATALOG_PATH = @"Sketches";
        private static readonly string CATALOG_FILE = Path.Combine(CATALOG_PATH, "catalog.xml");

        [InjectionConstructor]
        public SketchCatalog()
        {
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
                         from image in LoadVectorImageAsync(info.File)
                         from sketchData in LoadModellingDataAsync(info.ModelFile, image)
                         select sketchData;

            return result;
        }

        private IObservable<VectorImageData> LoadVectorImageAsync(string fileName)
        {
            return Observable.ToAsync<VectorImageData>(() => LoadSvg(fileName))();
        }

        private IObservable<SketchData> LoadModellingDataAsync(string modelFile, VectorImageData vectorImage)
        {
            Func<SketchData> loadAction = () =>
                {
                    SketchData sketchData;
                    if (File.Exists(modelFile))
                    {
                        using (var reader = new StreamReader(modelFile))
                        {
                            var serializer = new XmlSerializer(typeof(SketchData));
                            sketchData = (SketchData)serializer.Deserialize(reader);
                        }
                    }
                    else
                        sketchData = new SketchData();

                    sketchData.Points = vectorImage.Points;
                    sketchData.Polygons = vectorImage.Polygons;
                    sketchData.Polylines = vectorImage.PolyLines;
                    return sketchData;
                };

            return Observable.ToAsync(loadAction)();
        }

        public IObservable<Unit> SaveSketchAsync(string sketchName, SketchData sketchData)
        {
            var result =
                from info in GetSketchMetadataAsync(sketchName)
                from _ in SaveModellingDataAsync(info.ModelFile, sketchData)
                select default(Unit);
            return result;
        }

        private IObservable<Unit> SaveModellingDataAsync(string fileName, SketchData sketchData)
        {
            Action saveAction = () =>
                {
                    using (var writer = new StreamWriter(fileName))
                    {
                        var serializer = new XmlSerializer(typeof(SketchData));
                        serializer.Serialize(writer, sketchData);
                    }
                };

            return Observable.ToAsync(saveAction)();
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
                                   File = Path.Combine(CATALOG_PATH, (string)el.Attribute("file")),
                                   ModelFile = Path.Combine(CATALOG_PATH, (string)el.Attribute("modelFile"))
                               };
                return metadata.ToArray();
            };

            return Observable.ToAsync(loadCatalog)();
        }


        private class SketchMetadata
        {
            public string Name { get; set; }
            public string File { get; set; }
            public string ModelFile { get; set; }
        }
    }
}
