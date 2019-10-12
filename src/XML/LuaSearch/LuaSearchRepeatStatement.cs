using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchRepeatStatement : XMLLuaSearchElement, IXMLLuaSearchStatement {
		public IXMLLuaSearchExpression Condition { get; set; }
		public XMLLuaSearchBlock Block { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Condition" && child.ChildNodes.Count > 0) {
                    var expr = child.ChildNodes[0];
                    Condition = LuaSearchXMLTypes.GetExpression(expr.Name, expr as XmlElement);
                } else if (child.Name == "Block") {
                    Block = new XMLLuaSearchBlock();
                    Block.FillIn(child as XmlElement);
                }
            }
        }
    }
}
