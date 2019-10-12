using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchGenericFor : XMLLuaSearchElement, IXMLLuaSearchStatement {
        public List<string> Variables { get; set; }
        public List<IXMLLuaSearchExpression> Expressions { get; set; }
        public XMLLuaSearchBlock Block { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Variables") {
                    Variables = new List<string>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[i];

                        if (innerchild is XmlText) {
                            Variables.Add(((XmlText)innerchild).InnerText);
                        }
                    }
                } else if (child.Name == "Expressions") {
                    Expressions = new List<IXMLLuaSearchExpression>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[i];

                        var expr = LuaSearchXMLTypes.GetExpression(innerchild.Name);
                        expr.FillIn(innerchild as XmlElement);
                        Expressions.Add(expr);
                    }
                } else if (child.Name == "Block") {
                    Block = new XMLLuaSearchBlock();
                    Block.FillIn(child as XmlElement);
                }
            }
        } 
    }
}
