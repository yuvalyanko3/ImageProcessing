using System;
using System.Drawing;

namespace AlgInterface
{
    public interface IImageProcessingAlgorithm
    {
        /// <summary>
        /// Manipulate the pixel data of originalImage
        /// </summary>
        Bitmap RunAlgorithm(Bitmap originalImage);
    }
}
