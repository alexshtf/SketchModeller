using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Utils;

using Enumerable = System.Linq.Enumerable;
using System.IO;

namespace SketchModeller.Utilities.Debugging
{
    public static class ArrayImage
    {
        public static void SaveScaledGray(double[,] data, string fileName)
        {
            var image = ScaledGray(data);
            SavePNG(image, fileName);
        }

        public static void SavePNG(Image image, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image.Source as BitmapSource));

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        public static Image ScaledGray(double[,] data)
        {
            var height = data.GetLength(1);
            var width = data.GetLength(0);
            var flat = data.Flatten();

            var min = flat.Min();
            var max = flat.Max();

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            if (max > min)
            {
                for (int row = 0; row < height; ++row)
                {
                    var rowBytesQuery = from col in Enumerable.Range(0, width)
                                        let value = (data[row, col] - min) / (max - min)
                                        select (byte)Math.Round(255 * value);
                    var rowBytes = rowBytesQuery.ToArray();
                    bitmap.WritePixels(new Int32Rect(0, row, width, 1), rowBytes, width, 0);
                }
            }
            else // the whole image is black
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), new byte[width * height], width, 0);

            return GetImage(width, height, bitmap);
        }

        public static Image Binary(bool[,] data)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);
            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);
            for (int row = 0; row < height; ++row)
            {
                var rowBytesQuery = from col in Enumerable.Range(0, width)
                                    select data[row, col] ? (byte)255 : (byte)0;
                var rowBytes = rowBytesQuery.ToArray();
                bitmap.WritePixels(new Int32Rect(0, row, width, 1), rowBytes, width, 0);
            }

            var image = GetImage(width, height, bitmap);
            return image;
        }

        private static Image GetImage(int width, int height, ImageSource imageSource)
        {
            var image = new Image { Source = imageSource, Width = width, Height = height };
            FWElementHelper.FakeLayout(image);
            return image;
        }
    }
}
