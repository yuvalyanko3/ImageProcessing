using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImagAlg.Model
{
    class UserImage
    {
        public UserImage(string imagePath)
        {
            this.Bitmap = new Bitmap(imagePath);
        }
        
        public Bitmap Bitmap { get; set; }
        public Bitmap ProcessedImage { get; set; }
    }
}
