using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedInterfaces;
using System.Xml;
using System.IO;

namespace LightBringer.Swatches
{
    /*
     * The swatch manager manges sets of swatches.  Swatches can be organized in a variety if sets and shared as needed.
     * A swatch set can be named by the user and saved accordingly such as "Pastelle", "Dark", etc.
     * Basically allowing the grouping of related swatches for easy lookup/etc
     */
    public class SwatchManager : IPersistence
    {
        private List<SwatchSet> m_sets;
        private const string m_SwatchSets = "SwatchSets";
        private const string m_setName = "Set";

        public SwatchManager()
        {
            m_sets = new List<SwatchSet>();
        }

        public void Load(XmlReader reader)
        {
            m_sets.Clear();

            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == m_setName)
                        {
                            SwatchSet set = new SwatchSet();

                            set.Load(reader);

                            m_sets.Add(set);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == m_SwatchSets)
                        {
                            break; // we are done
                        }
                    }
                }
            }


            catch (XmlException /*e*/)
            {
                // if the document is empty we just move on
                SwatchSet defaultSet = new SwatchSet();

                defaultSet.Name = "Default";
                m_sets.Add(defaultSet);
            }
        }

        public void Save(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(m_SwatchSets);

            foreach (SwatchSet set in m_sets)
            {
                writer.WriteStartElement(m_setName);
                set.Save(writer);
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

        public int Count
        {
            get
            {
                return m_sets.Count;
            }
        }

        public SwatchSet this[int index]
        {
            get
            {
                SwatchSet returnSet = null;

                if (index < m_sets.Count)
                {
                    returnSet = m_sets[index];
                }

                return returnSet; 
            }
        }

        public void Add(SwatchSet swatchSet)
        {
            if (m_sets.IndexOf(swatchSet) == -1)
            {
                m_sets.Add(swatchSet);
            }
        }

        public void Remove(SwatchSet swatchSet)
        {
            if (m_sets.IndexOf(swatchSet) != -1)
            {
                m_sets.Remove(swatchSet);
            }
        }
    }
}
