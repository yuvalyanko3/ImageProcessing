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
            this.bitmap = bitmap;
        }
        public Bitmap bitmap { get; set; }
    }
}
