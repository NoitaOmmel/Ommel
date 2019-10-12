using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlInclude(typeof(IXMLLuaSearchStatement))]
	public interface IXMLLuaSearchStatement {
        void FillIn(XmlElement elem);
    }
}
