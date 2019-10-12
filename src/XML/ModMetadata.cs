using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlRoot("Mod")]
    public class XMLModMetadata {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("author")]
        public string Author;

        [XmlAttribute("description")]
        public string Description;

        public void FillIn(XmlElement elem) {
            Name = elem.GetAttribute("name");
            Author = elem.GetAttribute("author");
            Description = elem.GetAttribute("description");
        }
    }
}
