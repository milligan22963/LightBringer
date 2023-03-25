using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using SharedInterfaces;

namespace LightBringer.Controllers
{
    /*
     * Each controller will be defined by a type which
     * indicates how many strips of lights they have
     * 
     * User's can use the defaults or create their own
     */
    [Serializable]
    public class ControllerType : IPersistence, ISerializable
    {
        #region DATA_REGION
        readonly ObservableCollection<ControllerStrip> m_strips;
        private const string m_controllerElement = "HardwareType";
        private const string m_stripCount = "NumberStrips";
        private const string m_stripElementName = "Strip";
        private const string m_stripGroupName = "Strips";
        private const string m_defaultLength = "DefaultLength";
        #endregion // DATA_REGION

        public ControllerType()
        {
            m_strips = new ObservableCollection<ControllerStrip>();
            DefaltStripLength = 24;
            ControllerName = "Default";
        }

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Add in movie count and each movie
            info.AddValue(m_controllerElement, ControllerName, typeof(string));
            info.AddValue(m_defaultLength, DefaltStripLength, typeof(int));
            info.AddValue(m_stripCount, m_strips.Count, typeof(int));

            foreach (ControllerStrip strip in m_strips)
            {
                strip.GetObjectData(info, context);
            }
        }

        // The special constructor is used to deserialize values. 
        public ControllerType(SerializationInfo info, StreamingContext context)
        {
            // restore number of pixels for this strip
            ControllerName = (string)info.GetValue(m_controllerElement, typeof(string));
            DefaltStripLength = (int)info.GetValue(m_defaultLength, typeof(int));

            int stripCount = (int)info.GetValue(m_stripCount, typeof(int));

            for (int count = 0; count < stripCount; count++)
            {
                m_strips.Add(new ControllerStrip(info, context));
            }
        }
        #endregion // SERIALIZATION

        #region PERSISTENCE
        public void Load(XmlReader reader)
        {
            m_strips.Clear(); // reset since we are restoring

            // Pull in my attribues then each strip
            ControllerName = reader.GetAttribute(m_controllerElement);
            DefaltStripLength = Convert.ToInt32(reader.GetAttribute(m_defaultLength));

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == m_stripElementName)
                    {
                        ControllerStrip strip = new ControllerStrip(DefaltStripLength);

                        strip.Load(reader);

                        m_strips.Add(strip);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_stripGroupName)
                    {
                        break; // we are done
                    }
                }
            }
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString(m_controllerElement, ControllerName);
            writer.WriteAttributeString(m_defaultLength, DefaltStripLength.ToString());
            writer.WriteStartElement(m_stripGroupName);
            foreach (ControllerStrip strip in m_strips)
            {
                writer.WriteStartElement(m_stripElementName);
                strip.Save(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void Load(BinaryReader reader)
        {
            // Unused for now
        }

        public void Save(BinaryWriter writer)
        {
            // unused for now
        }
        #endregion // PERSISTENCE

        #region DATA_MEMBERS

        virtual public string ControllerName
        {
            get;
            protected set;
        }

        virtual public int DefaltStripLength
        {
            get;
            protected set;
        }

        public ObservableCollection<ControllerStrip> Strips
        {
            get
            {
                return m_strips;
            }
        }

        public int NumberOfStrips
        {
            get
            {
                return m_strips.Count;
            }
            set
            {
                // Adjust
                if (value > m_strips.Count)
                {
                    // add in
                    while (m_strips.Count < value)
                    {
                        AddStrip(DefaltStripLength);
                    }
                }
                else if (value < m_strips.Count)
                {
                    // remove
                    while (m_strips.Count > value)
                    {
                        RemoveStrip(m_strips.Count - 1);
                    }
                }
            }
        }

        public ControllerStrip this[int index]
        {
            get
            {
                return m_strips[index];
            }
        }
        #endregion // DATA_MEMBERS

        public virtual ControllerStrip AddStrip(int stripLength)
        {
            ControllerStrip strip = new ControllerStrip(stripLength);

            m_strips.Add(strip);

            return strip;
        }

        public virtual void RemoveStrip(int stripIndex)
        {
            m_strips.RemoveAt(stripIndex);
        }

        public override string ToString()
        {
            return ControllerName;
        }
    }
}
