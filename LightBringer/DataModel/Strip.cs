using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace LightBringer.DataModel
{
    [Serializable]
    public class Strip : DataModelBase
    {
        #region DATA

        private readonly List<Pixel> m_pixels = new List<Pixel>();
        private const string m_PixelElementName = "Pixel";
        private const string m_PixelContainerName = "Pixels";
        private const string m_StripIdElementName = "StripId";

        #endregion //DATA

        #region CONSTRUCTOR
        public Strip()
        {
            DataName = "Strip";
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        override public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); // call base to serialize the name
            info.AddValue(m_StripIdElementName, StripId, typeof(int));

            // Persist the pixel count and each pixel
            info.AddValue(m_PixelContainerName, m_pixels.Count, typeof(int));
            foreach (Pixel pixel in m_pixels)
            {
                pixel.GetObjectData(info, context);
            }
        }

        // The special constructor is used to deserialize values. 
        public Strip(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StripId = (int)info.GetValue(m_StripIdElementName, typeof(int));

            // restore number of pixels for this strip
            int pixelCount = (int)info.GetValue(m_PixelContainerName, typeof(int));

            for (int count = 0; count < pixelCount; count++)
            {
                m_pixels.Add(new Pixel(info, context));
            }
        }
        #endregion // SERIALIZATION

        #region DATA_FIELDS

        public List<Pixel> Pixels
        {
            get
            {
                return m_pixels;
            }
        }
        #endregion // DATA_FIELDS

        public void SetColor(Color pixelColor, bool selectedOnly)
        {
            foreach (Pixel pixel in m_pixels)
            {
                if (selectedOnly == false || pixel.IsSelected == true)
                {
                    pixel.PixelColor = pixelColor;
                }
            }
        }

        public void SetColor(int pixelId, Color pixelColor)
        {
            if (pixelId < m_pixels.Count)
            {
                m_pixels[pixelId].PixelColor = pixelColor;
            }
        }

        public int StripId
        {
            get;
            set;
        }

        override public DataModelBase AddChild()
        {
            Pixel childObject = new Pixel();

            m_pixels.Add(childObject);

            return childObject; // base does nothing
        }

        #region PERSISTENCE
        override public void Load(XmlReader reader)
        {
            base.Load(reader);

            StripId = Convert.ToInt32(reader.GetAttribute(m_StripIdElementName));
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == m_PixelElementName)
                    {
                        Pixel pixel = new Pixel();

                        pixel.Load(reader);

                        m_pixels.Add(pixel);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_PixelContainerName)
                    {
                        break; // we are done
                    }
                }
            }
        }

        override public void Save(XmlWriter writer)
        {
            base.Save(writer);

            writer.WriteAttributeString(m_StripIdElementName, StripId.ToString());
            writer.WriteStartElement(m_PixelContainerName); // all pixels are stored in a container of pixels

            foreach (Pixel pixel in m_pixels)
            {
                writer.WriteStartElement(m_PixelElementName);
                pixel.Save(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        override public void Load(BinaryReader reader)
        {
            // Unused for now
        }

        override public void Save(BinaryWriter writer)
        {
            // unused for now
        }
        #endregion // PERSISTENCE
    }
}
