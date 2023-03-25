using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

namespace MostRecentFiles
{
    public class MRUFileHandler : INotifyPropertyChanged
    {

        #region DATA
        readonly ObservableCollection<FileListEntry> m_children;
        int m_maxFiles;
        const string m_MRUList = "MruList";
        const string m_fileEntry = "FileEntry";
        const string m_maxFilesAttribute = "MaxFiles";
        bool m_hasChanged = false;

        #endregion // DATA

        public MRUFileHandler()
        {
            m_children = new ObservableCollection<FileListEntry>();

            m_maxFiles = 5;
            UsePrivateFile = false;
        }

        public void AddFile(string fileName)
        {
            bool changesMade = true;
            bool addIt = true;

            // see if the file exists, if it does just update the last access time, and move it to the front
            // otherwise add it in
            if (m_children.Count > 0)
            {
                FileListEntry entry = null;

                try
                {
                    entry = m_children.First(fileListEntry => fileListEntry.FileName == fileName);
                }
                    // if it happens to not be there, cool, just add it below
                catch (InvalidOperationException /*e*/)
                {
                    entry = null;
                }
                if (entry != null)
                {
                    entry.LastAccessTime = DateTime.Now;

                    int oldIndex = m_children.IndexOf(entry);

                    // Move it to the front - we know there is at least one so if it isn't the first, make it so
                    if (oldIndex != 0)
                    {
                        m_children.Move(oldIndex, 0);
                    }
                    else
                    {
                        changesMade = false;
                    }
                    addIt = false;
                }
            }

            if (addIt == true)
            {
                // Add this one in
                m_children.Insert(0, new FileListEntry(fileName)); // put it at the front

                if (m_maxFiles < m_children.Count)
                {
                    m_children.RemoveAt(m_children.Count - 1);
                }
            }

            // If we have made changes then set our internal flag
            if (changesMade == true)
            {
                int index = 1;
                foreach (FileListEntry entry in m_children)
                {
                    entry.Id = index++;
                }

                m_hasChanged = true;
//                SaveMRU(MRUFileName);
            }
        }

        public void UpdateFile(string fileName)
        {
            AddFile(fileName);
        }

        #region DATA_PROPERTIES

        /*
         * Gets or sets a flag indicating if the most recent file data should be in the settings or stored in
         * a private file i.e. in the temporary application folder
         */
        public bool UsePrivateFile
        {
            get;
            set;
        }

        public string MRUFileName
        {
            get;
            set;
        }

        public FileListEntry this[int index]
        {
            get
            {
                FileListEntry returnValue = null;

                if (index < m_children.Count)
                {
                    returnValue = m_children[index];
                }
                return returnValue;
            }

            set
            {
                if (index < m_children.Count)
                {
                    m_children[index] = value;
                }
                else
                {
                    m_children.Add(value);
                }
            }
        }

        public int MaxFiles
        {
            get
            {
                return m_maxFiles;
            }
            set
            {
                // If we are shrinking the list
                // we need to make it so
                while (value < m_children.Count)
                {
                    m_children.RemoveAt(m_children.Count - 1); // remove the last one
                }
                m_maxFiles = value;
                m_hasChanged = true;
            }
        }

        public ObservableCollection<FileListEntry> Children
        {
            get
            {
                return m_children;
            }
        }
        #endregion

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

        #region PERSISTENCE
        public void SaveMRU()
        {
            if (m_hasChanged == true)
            {
                if (UsePrivateFile == true)
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;
                    settings.NewLineOnAttributes = true;

                    XmlWriter writer = XmlWriter.Create(MRUFileName, settings);
                    writer.WriteStartDocument();
                    writer.WriteStartElement(m_MRUList);

                    writer.WriteAttributeString(m_maxFilesAttribute, m_maxFiles.ToString());

                    // Now persist it
                    foreach (FileListEntry entry in m_children)
                    {
                        writer.WriteStartElement(m_fileEntry);
                        entry.SaveFileEntry(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.Close();
                }
                else
                {
                    Properties.Settings mruSettings = new Properties.Settings();

                    mruSettings.MaxFiles = m_maxFiles;
                    for (int index = 0; index < m_children.Count; index++)
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    mruSettings.MRUOne = m_children[index].FileName;
                                }
                                break;
                            case 1:
                                {
                                    mruSettings.MRUTwo = m_children[index].FileName;
                                }
                                break;
                            case 2:
                                {
                                    mruSettings.MRUThree = m_children[index].FileName;
                                }
                                break;
                            case 3:
                                {
                                    mruSettings.MRUFour = m_children[index].FileName;
                                }
                                break;
                            case 4:
                                {
                                    mruSettings.MRUFive = m_children[index].FileName;
                                }
                                break;
                        }
                    }
                    mruSettings.Save();
                }

                m_hasChanged = false;
            }
        }

        public void RestoreMRU()
        {
            m_children.Clear();

            if (UsePrivateFile == true)
            {
                if (File.Exists(MRUFileName) == false)
                {
                    FileStream fileStream = File.Create(MRUFileName);

                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }

                XmlReaderSettings settings = new XmlReaderSettings();

                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlReader reader = XmlReader.Create(MRUFileName, settings);

                try
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == m_fileEntry)
                            {
                                FileListEntry entry = new FileListEntry();

                                entry.RestoreFileEntry(reader);
                                m_children.Insert(m_children.Count, entry); // add each one on the end

                                entry.Id = m_children.Count; // set the id for this one
                            }
                            else if (reader.Name == m_MRUList)
                            {
                                m_maxFiles = Convert.ToInt32(reader.GetAttribute(m_maxFilesAttribute));
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (reader.Name == m_MRUList)
                            {
                                break; // we are done
                            }
                        }
                    }
                }

                catch (XmlException /*e*/)
                {
                    // if the document is empty we just move on
                }
                reader.Close();
            }
            else
            {
                Properties.Settings mruSettings = new Properties.Settings();

                m_maxFiles = Math.Max(mruSettings.MaxFiles, 5);
                for (int index = 0; index < m_maxFiles; index++)
                {
                    string fileName = null;
                    switch (index)
                    {
                        case 0:
                            {
                                fileName = mruSettings.MRUOne;
                            }
                            break;
                        case 1:
                            {
                                fileName = mruSettings.MRUTwo;
                            }
                            break;

                        case 2:
                            {
                                fileName = mruSettings.MRUThree;
                            }
                            break;
                        case 3:
                            {
                                fileName = mruSettings.MRUFour;
                            }
                            break;
                        case 4:
                            {
                                fileName = mruSettings.MRUFive;
                            }
                            break;
                    }
                    if (string.IsNullOrEmpty(fileName) == false)
                    {
                        FileListEntry entry = new FileListEntry();

                        entry.FileName = fileName;
                        m_children.Insert(m_children.Count, entry); // add each one on the end

                        entry.Id = m_children.Count; // set the id for this one
                    }
                }
            }
            m_hasChanged = false;
        }
        #endregion // PERSISTENCE
    }
}
