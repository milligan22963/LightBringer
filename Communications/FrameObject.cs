using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Communications
{
    public class FrameObject
    {
        public FrameObject()
        {
        }

        public int Length()
        {
            int commandLength = sizeof(Int16); // duration

            if (Pixels != null)
            {
                commandLength += sizeof(byte); // length of strip
                commandLength += sizeof(byte) * 3 * Pixels.Length; // 3 color bytes per pixel
            }

            return commandLength;
        }

        public void PrepareToSend(byte[] dataStream, int offset)
        {
            byte[] tempBytes = BitConverter.GetBytes(Duration);
            tempBytes.CopyTo(dataStream, offset);
            offset += tempBytes.Length;

            if (Pixels != null)
            {
                dataStream[offset++] = (byte)Pixels.Length;
                foreach (Color color in Pixels)
                {
                    double colorPercent = color.A / 255.0;

                    // drop each color
                    dataStream[offset++] = (byte)(color.R * colorPercent);
                    dataStream[offset++] = (byte)(color.G * colorPercent);
                    dataStream[offset++] = (byte)(color.B * colorPercent);
                }
            }
            else
            {
                dataStream[offset++] = 0;
            }
        }

        public void ReceiveData(byte[] dataStream, int offset)
        {
            Duration = Convert.ToInt16(dataStream[offset]);
            offset += sizeof(Int16);

            byte stripLength = dataStream[offset++];
            if (stripLength > 0)
            {
                Color[] pixels = new Color[stripLength];
                for (int pixel = 0; pixel < stripLength; pixel++)
                {
                    pixels[pixel] = new Color();
                    pixels[pixel].R = dataStream[offset++];
                    pixels[pixel].G = dataStream[offset++];
                    pixels[pixel].B = dataStream[offset++];
                    pixels[pixel].A = 255;  // we keep the opacity on the server side so by default it will be whatever it is at full
                }

                Pixels = pixels;
            }
            else
            {
                Pixels = null;
            }
        }

        #region PROPERTIES
        
        /// <summary>
        /// Gets or sets the duration of this frame when viewed in milliseconds.
        /// </summary>
        public Int16 Duration
        {
            get;
            set;
        }

        public Color[] Pixels
        {
            get;
            set;
        }
        #endregion
    }
}
