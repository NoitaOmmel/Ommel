using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchIfStatement : XMLLuaSearchElement, IXMLLuaSearchStatement {
        public IXMLLuaSearchExpression Condition { get; set; }
        public XMLLuaSearchBlock Block { get; set; }
        public List<XMLLuaSearchIfStatement> ElseIfs { get; set; }
        public XMLLuaSearchBlock Else { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Condition" && child.ChildNodes.Count > 0) {
                    var cond = child.ChildNodes[0];
                    Condition = LuaSearchXMLTypes.GetExpression(cond.Name, cond as XmlElement);
                } else if (child.Name == "Block") {
                    Block = new XMLLuaSearchBlock();
                    Block.FillIn(child as XmlElement);
                } else if (child.Name == "ElseBlock") {
                    Else = new XMLLuaSearchBlock();
                    Else.FillIn(child as XmlElement);
                } else if (child.Name == "ElseIfs") {
                    ElseIfs = new List<XMLLuaSearchIfStatement>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        var ifstat = new XMLLuaSearchIfStatement();
                        ifstat.FillIn(innerchild as XmlElement);
                    }
                }
            }
        }
    }
}
