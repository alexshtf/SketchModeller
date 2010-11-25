using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Services
{
    /// <summary>
    /// A sketch processing service. Responsible for loading sketches and extracting sketch data.
    /// </summary>
    [ContractClass(typeof(SketchProcessingContract))]
    public interface ISketchProcessing
    {

        /// <summary>
        /// Creates a task that asynchronously loads an image to a double-array.
        /// </summary>
        /// <param name="fileName">The file to load the image from.</param>
        /// <returns>A task that computes a two-dimensional array that represents gray 
        /// levels of pixels on the image in range [0..1].</returns>
        /// <remarks>
        /// This method supports only grayscale images. Color images and black-white images are automatically
        /// converted to grayscale.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileName"/> is null.</exception>
        IObservable<double[,]> LoadSketchImageAsync(string fileName);

        /// <summary>
        /// Creates a task that asynchronously extracts meaningful information from the sketch.
        /// </summary>
        /// <param name="image">A two-dimensional array of a grayscale image.</param>
        /// <returns>A task that computes meaningful information from the image. </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
        /// <exception cref="OutOfRangeException">Thrown when <paramref name="image"/> contains values that
        /// are not in range [0..1].</exception>
        IObservable<SketchData> ProcessSketchImageAsync(double[,] image);
    }

    [ContractClassFor(typeof(ISketchProcessing))]
    public abstract class SketchProcessingContract : ISketchProcessing
    {

        public IObservable<double[,]> LoadSketchImageAsync(string fileName)
        {
            Contract.Requires(fileName != null);
            Contract.Requires(fileName != string.Empty);
            Contract.Ensures(Contract.Result<IObservable<double[,]>>() != null);
            return null;
        }

        public IObservable<SketchData> ProcessSketchImageAsync(double[,] image)
        {
            Contract.Requires(image != null);
            Contract.Requires(Contract.ForAll(image.Cast<double>(), pixel => pixel >= 0 && pixel <= 1));
            Contract.Ensures(Contract.Result<IObservable<SketchData>>() != null);

            return null;
        }
    }
}
