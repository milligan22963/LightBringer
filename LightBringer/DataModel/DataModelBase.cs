using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using SharedInterfaces;

namespace LightBringer.DataModel
{
    [Serializable]
    public class DataModelBase : IPersistence, ISerializable
    {
        #region DATA

        private readonly Regex m_nameExpression = new Regex(@"^[A-Za-z ]+$");
        private const string m_Name = "Name";
        private const string m_Id = "Id";
        static private int m_nextId = 1; // start at one and move up from there

        #endregion // DATA

        #region CONSTRUCTOR
        public DataModelBase()
        {
            IsDirty = false;

            Id = m_nextId++;
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        virtual public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(m_Name, DataName, typeof(string));
            info.AddValue(m_Id, Id, typeof(int));
        }

        // The special constructor is used to deserialize values. 
        public DataModelBase(SerializationInfo info, StreamingContext context)
        {
            DataName = (string)info.GetValue(m_Name, typeof(string));
            Id = (int)info.GetValue(m_Id, typeof(int));
        }
        #endregion // SERIALIZATION

        #region DATA_FIELDS
        virtual public string DataName
        {
            get;
            set;
        }

        public bool IsDirty
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public virtual string this[string columnName]
        {
            get
            {
                string returnString = String.Empty;

                // apply property level validation rules
                if (columnName == "Name")
                {
                    if (DataName == null || DataName == string.Empty)
                    {
                        returnString = "Name cannot be null or empty";
                    }

                    if (m_nameExpression.Match(DataName).Success == false)
                    {
                        returnString = "Name may only contain characters or spaces";
                    }
                }

                return returnString;
            }
        }

        #endregion // DATA_FIELDS

        #region COMMON METHODS
        public void SetName(string name)
        {
            // handle the name change
            DataName = name;
        }

        virtual public DataModelBase AddChild()
        {
            return null; // base does nothing
        }
        #endregion // common methods

        #region PERSISTENCE
        virtual public void Load(XmlReader reader)
        {
            DataName = reader.GetAttribute(m_Name);
        }

        virtual public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString(m_Name, DataName);
        }

        virtual public void Load(BinaryReader reader)
        {
            // Unused for now
        }

        virtual public void Save(BinaryWriter writer)
        {
            // unused for now
        }
        #endregion // PERSISTENCE
    }
}
