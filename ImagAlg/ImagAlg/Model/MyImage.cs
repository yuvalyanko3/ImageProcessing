using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImagAlg.Model
{
    class MyImage
    {
        public MyImage(Bitmap bitmap)
        {
            this.Bitmap = bitmap;
        }
        public Bitmap Bitmap { get; set; }
        public Bitmap ProcessedImage { get; set; }
    }
}
