using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchFunctionCall : XMLLuaSearchElement, IXMLLuaSearchExpression, IXMLLuaSearchStatement {
		public IXMLLuaSearchExpression Function { get; set; }
		public List<IXMLLuaSearchExpression> Arguments { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Function" && child.ChildNodes.Count > 0) {
                    var func = child.ChildNodes[i];
                    Function = LuaSearchXMLTypes.GetExpression(func.Name);
                    Function.FillIn(func as XmlElement);
                } else if (child.Name == "Arguments") {
                    Arguments = new List<IXMLLuaSearchExpression>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        var expr = LuaSearchXMLTypes.GetExpression(innerchild.Name);
                        expr.FillIn(innerchild as XmlElement);
                        Arguments.Add(expr);
                    }
                }
            }
        }
    }
}
