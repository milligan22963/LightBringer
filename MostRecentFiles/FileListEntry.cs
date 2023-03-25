using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Windows.Controls;
using System.IO;

namespace MostRecentFiles
{
    public class FileListEntry : INotifyPropertyChanged
    {
        #region DATA
        string m_fileName;
        DateTime m_lastAccessTime;
        const string m_fileNameAttribute = "FileName";
        const string m_lastAccessTimeAttribute = "LastAccessTime";
        #endregion // DATA

        #region CONSTRUCTOR
        public FileListEntry()
        {
            m_fileName = "none";
        }

        public FileListEntry(string fileName)
        {
            FileName = fileName;
            m_lastAccessTime = DateTime.Now;
        }
        #endregion // CONSTRUCTOR

        #region DATA_PROPERTIES
        public string FileName
        {
            get
            {
                return m_fileName;
            }

            set
            {
                m_fileName = value;
                m_lastAccessTime = DateTime.Now;

                OnPropertyChanged("FileName");
            }
        }

        public int Id
        {
            get;
            set;
        }

        public string Header
        {
            get
            {
                string menuHeader = Id.ToString() + " - " + Path.GetFileNameWithoutExtension(m_fileName);

                return menuHeader;
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return m_lastAccessTime;
            }
            set
            {
                m_lastAccessTime = value;
                OnPropertyChanged("LastAccessTime");
            }
        }
        #endregion // DATA_PROPERTIES

        #region PERSISTENCE
        public void SaveFileEntry(XmlWriter writer)
        {
            writer.WriteAttributeString(m_fileNameAttribute, m_fileName);
            writer.WriteAttributeString(m_lastAccessTimeAttribute, m_lastAccessTime.Ticks.ToString());
        }

        public void RestoreFileEntry(XmlReader reader)
        {
            // Doing it here directly because we don't want to change the last access time
            m_fileName = reader.GetAttribute(m_fileNameAttribute);
            m_lastAccessTime = new DateTime(Convert.ToInt64(reader.GetAttribute(m_lastAccessTimeAttribute)));
        }
        #endregion // PERSISTENCE
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
