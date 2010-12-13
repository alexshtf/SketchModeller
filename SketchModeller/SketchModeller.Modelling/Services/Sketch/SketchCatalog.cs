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
                         from sketchData in sketchProcessing.ProcessSketchImageAsync(image)
                         select sketchData;

            return result;
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
