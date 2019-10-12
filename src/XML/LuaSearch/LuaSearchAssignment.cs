using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlRoot("Assignment")]
	public class XMLLuaSearchAssignment : XMLLuaSearchElement, IXMLLuaSearchStatement {
        public List<IXMLLuaSearchAssignable> Variables { get; set; }
        public List<IXMLLuaSearchExpression> Expressions { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Variables") {
                    Variables = new List<IXMLLuaSearchAssignable>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        var ass = LuaSearchXMLTypes.GetAssignable(innerchild.Name);
                        ass.FillIn(innerchild as XmlElement);

                        Variables.Add(ass);
                    }
                } else if (child.Name == "Expressions") {
                    Expressions = new List<IXMLLuaSearchExpression>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        var expr = LuaSearchXMLTypes.GetExpression(innerchild.Name);
                        expr.FillIn(innerchild as XmlElement);

                        Expressions.Add(expr);
                    }
                }
            }
        }
    }
}
