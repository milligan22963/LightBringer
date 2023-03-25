using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace SharedInterfaces
{
    public interface IPersistence
    {
        void Load(XmlReader reader);
        void Save(XmlWriter writer);

        void Load(BinaryReader reader);
        void Save(BinaryWriter writer);
    }
}
