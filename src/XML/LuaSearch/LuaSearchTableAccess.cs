using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchTableAccess : XMLLuaSearchElement, IXMLLuaSearchExpression, IXMLLuaSearchAssignable {
		public IXMLLuaSearchExpression Table { get; set; }
		public IXMLLuaSearchExpression Index { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Table" && child.ChildNodes.Count > 0) {
                    var tab = child.ChildNodes[0];
                    Table = LuaSearchXMLTypes.GetExpression(tab.Name, tab as XmlElement);
                } else if (child.Name == "Index" && child.ChildNodes.Count > 0) {
                    var idx = child.ChildNodes[0];
                    Index = LuaSearchXMLTypes.GetExpression(idx.Name, idx as XmlElement);
                }
            }
        }
    }
}
