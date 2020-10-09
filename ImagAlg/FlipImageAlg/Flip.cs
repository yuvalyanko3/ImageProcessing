using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgInterface;

namespace FlipImageAlg
{
    public class Flip : IImageProcessingAlgorithm
    {
        public Bitmap RunAlgorithm(Bitmap originalImage)
        {
            originalImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            return originalImage;
        }
    }
}
