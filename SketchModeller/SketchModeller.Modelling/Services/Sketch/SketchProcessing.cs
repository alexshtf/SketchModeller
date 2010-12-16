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

using WFPixelFormat = System.Drawing.Imaging.PixelFormat;
using WFBitmap = System.Drawing.Bitmap;
using WFColor = System.Drawing.Color;
using AForge.Imaging.Filters;

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
            Func<SketchData> process = () =>
                {
                    var w = image.GetLength(0);
                    var h = image.GetLength(1);

                    var bmp = new WFBitmap(w, h, WFPixelFormat.Format32bppArgb);
                    for (var x = 0; x < w; ++x)
                        for (var y = 0; y < h; ++y)
                        {
                            var value = (int)Math.Round(255 * image[x, y]);
                            var color = WFColor.FromArgb(value, value, value);
                            bmp.SetPixel(x, y, color);
                        }

                    bmp = Grayscale.CommonAlgorithms.BT709.Apply(bmp);
                    bmp = new FlatFieldCorrection().Apply(bmp);
                    bmp = new ContrastStretch().Apply(bmp);
                    bmp = new OtsuThreshold().Apply(bmp);
                    var points = new List<Point>();
                    for (int x = 0; x < bmp.Width; ++x)
                        for (int y = 0; y < bmp.Height; ++y)
                        {
                            var color = bmp.GetPixel(x, y);
                            var brightness = color.GetBrightness();
                            if (brightness < 0.1)
                            {
                                var xNormalized = 2 * x / (double)bmp.Width - 1;
                                var yNormalized = 1 - 2 * y / (double)bmp.Height;
                                points.Add(new Point { X = xNormalized, Y = yNormalized });
                            }
                        }

                    return new SketchData 
                    { 
                        Image = image, 
                        Points = points.ToArray(),
                    };
                };

            var observable = Observable.FromAsyncPattern<SketchData>(process.BeginInvoke, process.EndInvoke)();
            return observable;
        }


    }
}
