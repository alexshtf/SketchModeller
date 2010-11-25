using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Utils;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView : UserControl
    {
        public SketchImageView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchImageView(SketchImageViewModel viewModel)
            : this()
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            e.Match(() => ViewModel.ImageData, () =>
                {
                    // get image information
                    var imageData = ViewModel.ImageData;
                    var width = imageData.GetLength(0);
                    var height = imageData.GetLength(1);

                    // convert the image data to a 1D array of float values.
                    float[] floatPixelsArray = new float[width * height];
                    for (int i = 0; i < width; ++i)
                        for (int j = 0; j < height; ++j)
                            floatPixelsArray[i + j * width] = (float)imageData[i, j];

                    // create bitmap source from the float values
                    var writableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray32Float, null);
                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), floatPixelsArray, width * sizeof(float), 0);
                    writableBitmap.Freeze();

                    // display the image
                    image.Source = writableBitmap;
                });
        }

        private SketchImageViewModel ViewModel
        {
            get { return (SketchImageViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
