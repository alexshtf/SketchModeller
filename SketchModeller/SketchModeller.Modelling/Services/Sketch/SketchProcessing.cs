using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace SketchModeller.Modelling.Services.Sketch
{
    class SketchProcessing : ISketchProcessing
    {
        public IObservable<double[,]> LoadSketchImageAsync(string fileName)
        {
            Func<double[,]> loadAction = () =>
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        var grayBitmap = new FormatConvertedBitmap(
                            source: frame,
                            destinationFormat: PixelFormats.Gray32Float,
                            destinationPalette: null,
                            alphaThreshold: 1.0);

                        float[] pixels = new float[grayBitmap.PixelWidth * grayBitmap.PixelHeight];
                        grayBitmap.CopyPixels(
                            pixels: pixels,
                            stride: (grayBitmap.PixelWidth * grayBitmap.Format.BitsPerPixel + 7) / 8,
                            offset: 0);

                        double[,] result = new double[grayBitmap.PixelWidth, grayBitmap.PixelHeight];
                        for (int x = 0; x < grayBitmap.PixelWidth; ++x)
                            for (int y = 0; y < grayBitmap.PixelHeight; ++y)
                                result[x, y] = pixels[x + y * grayBitmap.PixelWidth];

                        return result;
                    }
                };

            var observable = Observable.FromAsyncPattern<double[,]>(loadAction.BeginInvoke, loadAction.EndInvoke)();
            return observable;
        }

        public IObservable<SketchData> ProcessSketchImageAsync(double[,] image)
        {
            return null;
        }


    }
}
