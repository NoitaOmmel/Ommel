using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchUnaryExpression : XMLLuaSearchElement, IXMLLuaSearchExpression {
		public IXMLLuaSearchExpression Expression { get; set; }
		public NetLua.Ast.UnaryOp Operation { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            var op = elem.GetAttribute("operation");
            if (op != null) Operation = (NetLua.Ast.UnaryOp)Enum.Parse(typeof(NetLua.Ast.UnaryOp), op, true);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Expression" && child.ChildNodes.Count > 0) {
                    var expr = child.ChildNodes[0];
                    Expression = LuaSearchXMLTypes.GetExpression(expr.Name, expr as XmlElement);
                }
            }
        }
    }
}
