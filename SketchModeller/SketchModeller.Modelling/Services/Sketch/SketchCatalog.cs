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
using System.Runtime.Serialization.Formatters.Binary;

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

        public IObservable<Unit> CreateSketchAsync(string sketchName, SketchData sketchData)
        {
            var result =
                from catalog in LoadCatalogAsync()
                from i1 in Observable.If(() => !ExistsSketch(catalog, sketchName), AddToCatalogAsync(catalog, sketchName))
                from i2 in SaveSketchAsync(sketchName, sketchData)
                select default(Unit);
            return result;
        }

        private IObservable<Unit> AddToCatalogAsync(SketchMetadata[] catalog, string sketchName)
        {
            var catalogList = new List<SketchMetadata>(catalog);
            catalogList.Add(new SketchMetadata
            {
                Name = sketchName,
                ModelFile = sketchName + ".model",
                File = string.Empty,
            });
            var document = new XDocument(
                new XElement("catalog",
                    from catalogItem in catalogList
                    select new XElement("sketch",
                        new XAttribute("name", catalogItem.Name),
                        new XAttribute("file", catalogItem.File),
                        new XAttribute("modelFile", catalogItem.ModelFile))
                    )
            );

            Action saveAction = () =>
                {
                    using (var stream = File.Create(CATALOG_FILE))
                    {
                        document.Save(stream);
                    }
                };
            return Observable.ToAsync(saveAction)();
        }

        private bool ExistsSketch(SketchMetadata[] catalog, string sketchName)
        {
            var query =
                from item in catalog
                where item.Name == sketchName
                select item;
            return query.Any();
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
                        using (var stream = File.OpenRead(modelFile))
                        {
                            var serializer = new BinaryFormatter();
                            sketchData = (SketchData)serializer.Deserialize(stream);
                        }
                    }
                    else
                        sketchData = new SketchData();

                    if (sketchData.Polylines == null)
                        sketchData.Polylines = vectorImage.PolyLines;

                    if (sketchData.Polygons == null)
                        sketchData.Polygons = vectorImage.Polygons;

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
                    using (var stream = File.Create(fileName))
                    {
                        var serializer = new BinaryFormatter();
                        serializer.Serialize(stream, sketchData);
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
