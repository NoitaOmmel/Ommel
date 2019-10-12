using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchVariable : XMLLuaSearchElement, IXMLLuaSearchExpression, IXMLLuaSearchAssignable {
		public string Name { get; set; }
		public IXMLLuaSearchExpression Prefix { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            Name = elem.GetAttribute("name");

            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Prefix" && child.ChildNodes.Count > 0) {
                    var expr = child.ChildNodes[0];
                    Prefix = LuaSearchXMLTypes.GetExpression(expr.Name, expr as XmlElement);
                }
            }
        }
    }
}
