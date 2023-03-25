using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using SharedInterfaces;

namespace LightBringer.DataModel
{
    [Serializable]
    public class Frame : DataModelBase
    {
        #region DATA

        readonly private List<Strip> m_strips = new List<Strip>();
        private const string m_StripElementName = "Strip";
        private const string m_StripContainerName = "Strips";
        private const string m_transformsElementName = "Transforms";
        private List<ITransform> m_transforms = new List<ITransform>();

        #endregion //DATA

        #region CONSTRUCTOR
        public Frame()
        {
            DataName = "Frame";
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        override public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); // call base to serialize the name

            // Persist the pixel count and each pixel
            info.AddValue(m_StripContainerName, m_strips.Count, typeof(int));
            foreach (Strip strip in m_strips)
            {
                strip.GetObjectData(info, context);
            }
        }

        // The special constructor is used to deserialize values. 
        public Frame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // restore number of pixels for this strip
            int stripCount = (int)info.GetValue(m_StripContainerName, typeof(int));

            for (int count = 0; count < stripCount; count++)
            {
                m_strips.Add(new Strip(info, context));
            }
        }
        #endregion // SERIALIZATION

        public override DataModelBase AddChild()
        {
            Strip childObject = new Strip();

            m_strips.Add(childObject);

            return childObject; // base does nothing
        }

        public void AddTransform(ITransform transform)
        {
            m_transforms.Add(transform);
            
            Transform.Transform baseTransform = transform as Transform.Transform;

            if (baseTransform != null)
            {
                baseTransform.PropertyChanged += Transform_PropertyChanged;
            }
            IsDirty = true;
        }

        void Transform_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }

        #region DATA_FIELDS

        public List<Strip> Strips
        {
            get
            {
                return m_strips;
            }
        }

        public List<ITransform> GetTransforms
        {
            get
            {
                return m_transforms;
            }
        }

        public int Transforms
        {
            get
            {
                return m_transforms.Count;
            }
        }
        #endregion // DATA_FIELDS

        #region PERSISTENCE
        override public void Load(XmlReader reader)
        {
            base.Load(reader);

            m_transforms.Clear();

            bool loadingStrips = true;
            bool loadingTransforms = false;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if ((reader.Name == m_StripElementName) && (loadingStrips == true))
                    {
                        Strip strip = new Strip();

                        strip.Load(reader);

                        m_strips.Add(strip);
                    }
                    else if ((reader.Name == Transform.Transform.ElementName) && (loadingTransforms == true))
                    {
                        SharedInterfaces.TransformFactory factory = SharedInterfaces.TransformFactory.GetInstance();
                        string transformType = Transform.Transform.TransformType(reader);
                        SharedInterfaces.ITransform transform = factory.GetTransform(transformType);

                        if (transform != null)
                        {
                            transform.Load(reader);
                            transform.SetIconSize(SharedInterfaces.TransformIconSize.eSmall);
                            m_transforms.Add(transform);

                            Transform.Transform baseTransform = transform as Transform.Transform;

                            if (baseTransform != null)
                            {
                                baseTransform.PropertyChanged += Transform_PropertyChanged;
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_StripContainerName)
                    {
                        loadingStrips = false;
                        loadingTransforms = true;
                    }
                    else if (reader.Name == m_transformsElementName) // this might not appear
                    {
                        loadingTransforms = false;
                        break;
                    }
                    else if (loadingStrips == false) // we may still have strips to load
                    {
                        break; // done
                    }
                }
            }
        }

        override public void Save(XmlWriter writer)
        {
            base.Save(writer);

            writer.WriteStartElement(m_StripContainerName); // all strips are stored in a container of strips

            foreach (Strip strip in m_strips)
            {
                writer.WriteStartElement(m_StripElementName);
                strip.Save(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            if (m_transforms.Count > 0)
            {
                writer.WriteStartElement(m_transformsElementName);
                foreach (ITransform transform in m_transforms)
                {
                    transform.Save(writer);
                }
                writer.WriteEndElement();
            }
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
