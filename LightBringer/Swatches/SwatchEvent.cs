using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightBringer.Swatches
{
    public class SwatchEvent : EventArgs
    {
        public SwatchEvent()
        {
            Color = Colors.AliceBlue;
        }

        public SwatchEvent(Color colorValue)
        {
            Color = colorValue;
        }

        public Color Color
        {
            get;
            set;
        }   
    }
}
