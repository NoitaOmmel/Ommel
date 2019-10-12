using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlRoot("BinaryExpression")]
	public class XMLLuaSearchBinaryExpression : XMLLuaSearchElement, IXMLLuaSearchExpression {
		public IXMLLuaSearchExpression Left { get; set; }
		public IXMLLuaSearchExpression Right { get; set; }

        [XmlAttribute("operation")]
		public NetLua.Ast.BinaryOp Operation { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            var op = elem.GetAttribute("operation");
            if (op != null) Operation = (NetLua.Ast.BinaryOp)Enum.Parse(typeof(NetLua.Ast.BinaryOp), op, true);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Left" && child.ChildNodes.Count > 0) {
                    var left = child.ChildNodes[0];
                    Left = LuaSearchXMLTypes.GetExpression(left.Name);
                    Left.FillIn(left as XmlElement);
                } else if (child.Name == "Right" && child.ChildNodes.Count > 0) {
                    var right = child.ChildNodes[0];
                    Right = LuaSearchXMLTypes.GetExpression(right.Name);
                    Right.FillIn(right as XmlElement);
                }
            }
        }
    }
}
