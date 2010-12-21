﻿using System;
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
using System.Windows.Threading;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView 
    {
        private SketchImageViewModel viewModel;
        private DispatcherTimer timer;

        public SketchImageView()
        {
            InitializeComponent();
            timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (sender, args) =>
                {

                    timer.Stop();
                };
            timer.Start();
        }

        [InjectionConstructor]
        public SketchImageView(SketchImageViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            //grid.DataContext = viewModel;
            ViewModel3DHelper.InheritViewModel(this, viewModel);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // when visibility flags get updated
            e.Match(() => viewModel.IsImageShown, () => ShowHide(viewModel.IsImageShown, imageVisibilityTransform));
            e.Match(() => viewModel.IsSketchShown, () => ShowHide(viewModel.IsSketchShown, scatterVisibilityTransform));

            // when points get updated
            e.Match(() => viewModel.Points, () =>
                {
                    // IMPORTANT!!!! We assume that viewModel.ImageWidth and viewModel.ImageHeight have the correct
                    // values at this stage. That is, when we notify about points update we already have the image data.
                    var points3d = from pnt in viewModel.Points
                                   select new Point3D(pnt.X, pnt.Y, 0);
                    scatter.Points = new Point3DCollection(points3d);
                });

            // when image gets updated
            e.Match(() => viewModel.ImageData, () =>
                {
                    // get image information
                    var imageData = viewModel.ImageData;
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
                    imageBrush.ImageSource = writableBitmap;
                });
        }

        private void ShowHide(bool flag, ScaleTransform3D visibilityTransform)
        {
            double value = flag ? 1.0 : 0;
            visibilityTransform.ScaleX = visibilityTransform.ScaleY = visibilityTransform.ScaleZ = value;
        }
    }
}