using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchNumericFor : XMLLuaSearchElement, IXMLLuaSearchStatement {
		public IXMLLuaSearchExpression Var { get; set; }
		public IXMLLuaSearchExpression Limit { get; set; }
		public IXMLLuaSearchExpression Step { get; set; }
		public string Variable { get; set; }
		public XMLLuaSearchBlock Block { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);

            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Var" && child.ChildNodes.Count > 0) {
                    var @var = child.ChildNodes[0];
                    Var = LuaSearchXMLTypes.GetExpression(@var.Name, @var as XmlElement);
                } else if (child.Name == "Limit" && child.ChildNodes.Count > 0) {
                    var @var = child.ChildNodes[0];
                    Limit = LuaSearchXMLTypes.GetExpression(@var.Name, @var as XmlElement);
                } else if (child.Name == "Step" && child.ChildNodes.Count > 0) {
                    var @var = child.ChildNodes[0];
                    Step = LuaSearchXMLTypes.GetExpression(@var.Name, @var as XmlElement);
                } else if (child.Name == "Variable" && child.ChildNodes.Count > 0) {
                    var @var = child.ChildNodes[0];
                    if (!(@var is XmlText)) continue;
                    Variable = ((XmlText)@var).Value;
                } else if (child.Name == "Block") {
                    Block = new XMLLuaSearchBlock();
                    Block.FillIn(child as XmlElement);
                }
            }
        }
    }
}
