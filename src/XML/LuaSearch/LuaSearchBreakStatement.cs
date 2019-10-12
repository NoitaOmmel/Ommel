using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlRoot("BreakStatement")]
	public class XMLLuaSearchBreakStatement : XMLLuaSearchElement, IXMLLuaSearchStatement {
    }
}
