using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgInterface;

namespace InvertAlg
{
    public class Invert : IImageProcessingAlgorithm
    {
        public Bitmap RunAlgorithm(Bitmap originalImage)
        {
            originalImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return originalImage;
        }
    }
}
