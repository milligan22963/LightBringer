using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;

namespace LightBringer.DataModel
{
    [Serializable]
    public class Movie : DataModelBase
    {
        #region DATA

        readonly private List<Frame> m_Frames = new List<Frame>();
        private const string m_FrameElementName = "Frame";
        private const string m_FrameContainerName = "Frames";

        #endregion //DATA

        #region CONSTRUCTOR
        public Movie()
        {
            DataName = "Movie";
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        override public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); // call base to serialize the name

            // Persist the pixel count and each pixel
            info.AddValue(m_FrameContainerName, m_Frames.Count, typeof(int));
            foreach (Frame frame in m_Frames)
            {
                frame.GetObjectData(info, context);
            }
        }

        // The special constructor is used to deserialize values. 
        public Movie(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // restore number of pixels for this strip
            int FrameCount = (int)info.GetValue(m_FrameContainerName, typeof(int));

            for (int count = 0; count < FrameCount; count++)
            {
                m_Frames.Add(new Frame(info, context));
            }
        }
        #endregion // SERIALIZATION

        #region DATA_FIELDS

        public List<Frame> Frames
        {
            get
            {
                return m_Frames;
            }
        }
        #endregion // DATA_FIELDS

        override public DataModelBase AddChild()
        {
            Frame childObject = new Frame();

            m_Frames.Add(childObject);

            return childObject; // base does nothing
        }

        #region PERSISTENCE
        override public void Load(XmlReader reader)
        {
            base.Load(reader);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == m_FrameElementName)
                    {
                        Frame Frame = new Frame();

                        Frame.Load(reader);

                        m_Frames.Add(Frame);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_FrameContainerName)
                    {
                        break; // we are done
                    }
                }
            }
        }

        override public void Save(XmlWriter writer)
        {
            base.Save(writer);

            writer.WriteStartElement(m_FrameContainerName); // all Frames are stored in a container of Frames

            foreach (Frame frame in m_Frames)
            {
                writer.WriteStartElement(m_FrameElementName);
                frame.Save(writer);
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
