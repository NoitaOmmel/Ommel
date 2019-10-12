using System;
using System.Collections.Generic;
using System.Xml;

namespace Ommel {
	public class XMLLuaSearchBlock : XMLLuaSearchElement, IXMLLuaSearchStatement {
        public List<IXMLLuaSearchStatement> Statements { get; set; }

        public bool Full { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            Full = elem.GetAttribute("full") == "1";

            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Statements") {
                    Statements = new List<IXMLLuaSearchStatement>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        var st = LuaSearchXMLTypes.GetStatement(innerchild.Name);
                        if (st == null) throw new Exception($"Invalid Lua AST element '{innerchild.Name}'");
                        st.FillIn(innerchild as XmlElement);

                        Statements.Add(st);
                    }
                }
            }

        }
    }
}
