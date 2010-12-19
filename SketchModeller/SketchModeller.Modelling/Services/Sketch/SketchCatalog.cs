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
                         from image in sketchProcessing.LoadSketchImageAsync(info.File)
                         from sketchData in LoadProcessedSketchAsync(info.InfoFile, image)
                         select sketchData;

            return result;
        }

        public IObservable<Unit> SaveSketchAsync(string sketchName, SketchData sketchData)
        {
            var result =
                from info in GetSketchMetadataAsync(sketchName)
                from _ in SaveSketchDataAsync(info.InfoFile, sketchData)
                select default(Unit);
            return result;
        }

        private IObservable<Unit> SaveSketchDataAsync(string fileName, SketchData sketchData)
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
                                   InfoFile = Path.Combine(CATALOG_PATH, (string)el.Attribute("info")),
                               };
                return metadata.ToArray();
            };

            return Observable.ToAsync(loadCatalog)();
        }

        private IObservable<SketchData> LoadProcessedSketchAsync(string infoFile, double[,] image)
        {
            Func<SketchData> loadAction = () =>
                {
                    using (var reader = new StreamReader(infoFile))
                    {
                        var serializer = new XmlSerializer(typeof(SketchData));
                        var sketchData = (SketchData)serializer.Deserialize(reader);
                        sketchData.Image = image;
                        return sketchData;
                    }
                };

            return Observable.ToAsync(loadAction)();
        }

        private class SketchMetadata
        {
            public string Name { get; set; }
            public string File { get; set; }
            public string InfoFile { get; set; }
        }
    }
}
