using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    public class XMLLuaSearchBoolLiteral : XMLLuaSearchElement, IXMLLuaSearchExpression {
        public bool Value { get; set; }
        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            Value = elem.GetAttribute("value") == "1";
        }
    }
}
