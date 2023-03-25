using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharedInterfaces;
using System.Xml;
using System.IO;

namespace LightBringer.Swatches
{
    public delegate void SwatchSetEventHandler(object sender, EventArgs e);

    public class SwatchSet : IPersistence
    {
        public event SwatchSetEventHandler Modified = null;

        private List<Swatch> m_swatches;
        private const string m_Name = "Name";
        private const string m_swatchElementName = "Swatch";

        public SwatchSet()
        {
            // Configure
            m_swatches = new List<Swatch>();
            Name = "Null";
            SwatchPanel = new WrapPanel();
        }

        public void Load(XmlReader reader)
        {
            m_swatches.Clear();

            Name = reader.GetAttribute(m_Name);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == m_swatchElementName)
                    {
                        // Need to read each swatch in my set
                        Swatch colorSwatch = new Swatch();

                        colorSwatch.Load(reader);
                        m_swatches.Add(colorSwatch);

                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != m_swatchElementName)
                    {
                        break; // we are done
                    }
                }
            }
        }

        public void Save(XmlWriter writer)
        {

            writer.WriteAttributeString(m_Name, Name);

            foreach (Swatch swatch in m_swatches)
            {
                writer.WriteStartElement(m_swatchElementName);
                swatch.Save(writer);
                writer.WriteEndElement();
            }
        }

        public void Load(BinaryReader reader)
        {
            // Unused for now
        }

        public void Save(BinaryWriter writer)
        {
            // unused for now
        }

        public string Name
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return m_swatches.Count;
            }
        }

        public WrapPanel SwatchPanel
        {
            get;
            set;
        }

        public void headerText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox headerText = sender as TextBox;

            Name = headerText.Text;

            // Notify my listeners
            OnModified(e);

            e.Handled = true;
        }

        protected virtual void OnModified(EventArgs e)
        {
            if (Modified != null)
            {
                Modified(this, e);
            }
        }

        public Swatch this[int index]
        {
            get
            {
                Swatch returnSwatch = null;

                if (index < m_swatches.Count)
                {
                    returnSwatch = m_swatches[index];
                }

                return returnSwatch;
            }
        }

        public void Add(Swatch swatch)
        {
            if (m_swatches.IndexOf(swatch) == -1)
            {
                m_swatches.Add(swatch);
            }
        }

        public void Remove(Swatch swatch)
        {
            if (m_swatches.IndexOf(swatch) != -1)
            {
                m_swatches.Remove(swatch);
            }
        }
    }
}
