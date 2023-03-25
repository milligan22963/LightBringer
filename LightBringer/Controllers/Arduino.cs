using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LightBringer.Controllers
{
    [Serializable]
    public class Arduino : ControllerType
    {
        private int[] m_clockPins = { 4, 6, 8, 10 };
        private int[] m_dataPins = { 5, 7, 9, 11};

        public Arduino()
        {
            ControllerName = "Arduino";
            DefaltStripLength = 24;

            // DEfault to 4 strips
            AddStrip(DefaltStripLength);
            AddStrip(DefaltStripLength);
            AddStrip(DefaltStripLength);
            AddStrip(DefaltStripLength);

            int index = 0;
            foreach (ControllerStrip strip in Strips)
            {
                strip.ClockPin = m_clockPins[index];
                strip.DataPin = m_dataPins[index++];
                strip.Addressable = true;
                strip.SPI = false;
                strip.SelectPin = 0;
            }
        }

        public Arduino(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // When deserializing we don't want to add defauts
        }

        public override ControllerStrip AddStrip(int stripLength)
        {
            ControllerStrip strip = base.AddStrip(stripLength);

            strip.ClockPin = 0;
            strip.DataPin = 0;
            strip.Addressable = true;
            strip.SPI = false;
            strip.SelectPin = 0;

            return strip;
        }
    }
}
