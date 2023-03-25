using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBringer.Visuals
{
    public class PixelEventArgs : EventArgs
    {
        public PixelEventArgs()
        {
            Pixel = new Pixel();
        }

        public PixelEventArgs(Pixel pixel)
        {
            Pixel = pixel;
        }

        public Pixel Pixel
        {
            get;
            private set;
        }

    }
}
