using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchNumberLiteral : XMLLuaSearchElement, IXMLLuaSearchExpression {
		public double? Value { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            double val;
            if (double.TryParse(elem.GetAttribute("value"), out val)) {
                Value = Value;
            }
        }
    }
}
