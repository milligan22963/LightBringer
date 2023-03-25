using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using SharedInterfaces;

namespace LightBringer.Controllers
{
    [Serializable]
    public class ControllerStrip : INotifyPropertyChanged, IPersistence, ISerializable
    {
        #region DATA_REGION
        bool m_isAddressable;
        bool m_isSPI;
        int m_dataPin;
        int m_clockPin;
        int m_selectPin;

        private const string m_AddressableAttribute = "Addressable";
        private const string m_SPIAttribute = "SPI";
        private const string m_DataPinAttribute = "DataPin";
        private const string m_ClockPinAttribtue = "ClockPin";
        private const string m_SelectPinAttribute = "SelectPin";
        private const string m_StripNameAttribute = "Name";
        private const string m_StripLength = "Length";
        #endregion // DATA_REGION

        public ControllerStrip(int length)
        {
            StripName = "Strip";
            Length = length;
        }

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Add in each of our attributes
            info.AddValue(m_StripNameAttribute, StripName, typeof(string));
            info.AddValue(m_StripLength, Length, typeof(int));
            info.AddValue(m_AddressableAttribute, Addressable, typeof(bool));
            info.AddValue(m_SPIAttribute, SPI, typeof(bool));
            info.AddValue(m_DataPinAttribute, m_dataPin, typeof(int));
            info.AddValue(m_ClockPinAttribtue, m_clockPin, typeof(int));
            info.AddValue(m_SelectPinAttribute, m_selectPin, typeof(int));
        }

        // The special constructor is used to deserialize values. 
        public ControllerStrip(SerializationInfo info, StreamingContext context)
        {
            StripName = (string)info.GetValue(m_StripNameAttribute, typeof(string));
            Length = (int)info.GetValue(m_StripLength, typeof(int));
            Addressable = (bool)info.GetValue(m_AddressableAttribute, typeof(bool));
            SPI = (bool)info.GetValue(m_SPIAttribute, typeof(bool));
            m_dataPin = (int)info.GetValue(m_DataPinAttribute, typeof(int));
            m_clockPin = (int)info.GetValue(m_ClockPinAttribtue, typeof(int));
            m_selectPin = (int)info.GetValue(m_SelectPinAttribute, typeof(int));
        }

        #endregion // SERIALIZATION
        #region PERSISTENCE
        public void Load(XmlReader reader)
        {
            Addressable = Convert.ToBoolean(reader.GetAttribute(m_AddressableAttribute));
            SPI = Convert.ToBoolean(reader.GetAttribute(m_SPIAttribute));
            m_dataPin = Convert.ToInt32(reader.GetAttribute(m_DataPinAttribute));
            m_clockPin = Convert.ToInt32(reader.GetAttribute(m_ClockPinAttribtue));
            m_selectPin = Convert.ToInt32(reader.GetAttribute(m_SelectPinAttribute));
            StripName = reader.GetAttribute(m_StripNameAttribute);
            Length = Convert.ToInt32(reader.GetAttribute(m_StripLength));
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString(m_AddressableAttribute, Addressable.ToString());
            writer.WriteAttributeString(m_SPIAttribute, SPI.ToString());
            writer.WriteAttributeString(m_DataPinAttribute, m_dataPin.ToString());
            writer.WriteAttributeString(m_ClockPinAttribtue, m_clockPin.ToString());
            writer.WriteAttributeString(m_SelectPinAttribute, m_selectPin.ToString());
            writer.WriteAttributeString(m_StripNameAttribute, StripName);
            writer.WriteAttributeString(m_StripLength, Length.ToString());
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

        public string StripName
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        /*
         * Indicates if the LEDs are addressable or not
         */
        public bool Addressable
        {
            get
            {
                return m_isAddressable;
            }
            set
            {
                m_isAddressable = value;
                OnPropertyChanged("Addressable");
            }
        }

        public int DataPin
        {
            get
            {
                return m_dataPin;
            }
            set
            {
                m_dataPin = value;
                OnPropertyChanged("DataPin");
            }
        }

        public int ClockPin
        {
            get
            {
                return m_clockPin;
            }
            set
            {
                m_clockPin = value;
                OnPropertyChanged("ClockPin");
            }
        }

        public bool SPI
        {
            get
            {
                return m_isSPI;
            }
            set
            {
                if (value == false)
                {
                    BitBang = true;
                }
                else
                {
                    BitBang = false;
                }
                m_isSPI = value;
                OnPropertyChanged("SPI");
            }
        }

        protected bool BitBang
        {
            get;
            set;
        }

        public int SelectPin
        {
            get
            {
                return m_selectPin;
            }
            set
            {
                m_selectPin = value;
                OnPropertyChanged("SelectPin");
            }
        }

        #endregion // DATA_MEMBERS

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged Members
    }
}
