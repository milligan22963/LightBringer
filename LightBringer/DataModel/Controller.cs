using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace LightBringer.DataModel
{
    [Serializable]
    public class Controller : DataModelBase
    {
        #region DATA

        private readonly List<Movie> m_movies = new List<Movie>();
        private const string m_controllerElement = "Controller";
        private const string m_controllerTypeElement = "ControllerType";
        private const string m_MovieElementName = "Movie";

        #endregion //DATA

        #region CONSTRUCTOR
        public Controller()
        {
            DataName = "Controller";
            AssociatedControllerType = new Controllers.Arduino();
        }
        #endregion // CONSTRUCTOR

        #region SERIALIZATION
        // Implement this method to serialize data. The method is called  
        // on serialization. 
        override public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); // call base to serialize the name

            // Need to know type that we are restoring
            info.AddValue(m_controllerTypeElement, AssociatedControllerType.GetType(), typeof(Type));
            AssociatedControllerType.GetObjectData(info, context);

            // Add in movie count and each movie
            info.AddValue(m_MovieElementName, m_movies.Count, typeof(int));
            foreach (Movie movie in m_movies)
            {
                movie.GetObjectData(info, context);
            }

        }

        // The special constructor is used to deserialize values. 
        public Controller(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Type controllerType = (Type)info.GetValue(m_controllerTypeElement, typeof(Type));

            Assembly assembly = Assembly.GetExecutingAssembly();

            AssociatedControllerType = (Controllers.ControllerType)assembly.CreateInstance(controllerType.FullName, false, BindingFlags.CreateInstance, null, new object[] {info, context}, null, null);

            // restore number of pixels for this strip
            int movieCount = (int)info.GetValue(m_MovieElementName, typeof(int));

            for (int count = 0; count < movieCount; count++)
            {
                m_movies.Add(new Movie(info, context));
            }
        }
        #endregion // SERIALIZATION

        #region DATA_FIELDS

        public Controllers.ControllerType AssociatedControllerType
        {
            get;
            set;
        }

        public List<Movie> Movies
        {
            get
            {
                return m_movies;
            }
        }
        #endregion // DATA_FIELDS

        override public DataModelBase AddChild()
        {
            Movie childObject = new Movie();

            m_movies.Add(childObject);

            return childObject; // base does nothing
        }

        #region PERSISTENCE
        override public void Load(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == m_MovieElementName)
                    {
                        Movie movie = new Movie();

                        movie.Load(reader);

                        m_movies.Add(movie);
                    }
                    else if (reader.Name == m_controllerElement)
                    {
                        base.Load(reader);
                    }
                    else if (reader.Name == m_controllerTypeElement)
                    {
                        string controllerType = reader.GetAttribute(m_controllerTypeElement);
                        Assembly assembly = Assembly.GetExecutingAssembly();

                        AssociatedControllerType = (Controllers.ControllerType)assembly.CreateInstance(controllerType, false, BindingFlags.CreateInstance, null, null, null, null);

                        AssociatedControllerType.Load(reader);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == m_controllerElement)
                    {
                        break; // we are done
                    }
                }
            }
        }

        override public void Save(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(m_controllerElement);
            
            base.Save(writer);
            
            writer.WriteStartElement(m_controllerTypeElement);
            writer.WriteAttributeString(m_controllerTypeElement, AssociatedControllerType.GetType().ToString());
            AssociatedControllerType.Save(writer);
            writer.WriteEndElement();

            foreach (Movie movie in m_movies)
            {
                writer.WriteStartElement(m_MovieElementName);
                movie.Save(writer);
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
