using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace LightBringer.DataModel
{
    [Serializable]
    public class Pixel : DataModelBase
    {
        #region DATA
        private const string m_alphaAttribute = "Alpha";
        private const string m_redAttribute = "Red";
        private const string m_greenAttribute = "Green";
        private const string m_blueAttribute = "Blue";
        private const string m_enumerationId = "EnumerationId";
        #endregion // DATA

        #region CONSTRUCTOR
        public Pixel()
        {
            base.DataName = "Pixel";
            EnumerationId = 0;
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        override public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); // call base to serialize the name

            // Write out each of my color attributes
            info.AddValue(m_alphaAttribute, PixelColor.A.ToString());
            info.AddValue(m_redAttribute, PixelColor.R.ToString());
            info.AddValue(m_greenAttribute, PixelColor.G.ToString());
            info.AddValue(m_blueAttribute, PixelColor.B.ToString());
            info.AddValue(m_enumerationId, EnumerationId, typeof(int));
        }

        // The special constructor is used to deserialize values. 
        public Pixel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Color storedColor = new Color();

            storedColor.A = Convert.ToByte(info.GetValue(m_alphaAttribute, typeof(string)));
            storedColor.R = Convert.ToByte(info.GetValue(m_redAttribute, typeof(string)));
            storedColor.G = Convert.ToByte(info.GetValue(m_greenAttribute, typeof(string)));
            storedColor.B = Convert.ToByte(info.GetValue(m_blueAttribute, typeof(string)));
            EnumerationId = (int)info.GetValue(m_enumerationId, typeof(int));

            PixelColor = storedColor;
        }
        #endregion // SERIALIZATION

        #region DATA_FIELDS

        public Color PixelColor
        {
            get;
            set;
        }

        public int EnumerationId
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get;
            set;
        }

        #endregion // DATA_FIELDS

        #region PERSISTENCE
        override public void Load(XmlReader reader)
        {
            base.Load(reader);

            Color storedColor = new Color();

            storedColor.A = Convert.ToByte(reader.GetAttribute(m_alphaAttribute));
            storedColor.R = Convert.ToByte(reader.GetAttribute(m_redAttribute));
            storedColor.G = Convert.ToByte(reader.GetAttribute(m_greenAttribute));
            storedColor.B = Convert.ToByte(reader.GetAttribute(m_blueAttribute));
            EnumerationId = Convert.ToInt32(reader.GetAttribute(m_enumerationId));

            PixelColor = storedColor;
        }

        override public void Save(XmlWriter writer)
        {
            base.Save(writer);

            writer.WriteAttributeString(m_alphaAttribute, PixelColor.A.ToString());
            writer.WriteAttributeString(m_redAttribute, PixelColor.R.ToString());
            writer.WriteAttributeString(m_greenAttribute, PixelColor.G.ToString());
            writer.WriteAttributeString(m_blueAttribute, PixelColor.B.ToString());
            writer.WriteAttributeString(m_enumerationId, EnumerationId.ToString());
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
