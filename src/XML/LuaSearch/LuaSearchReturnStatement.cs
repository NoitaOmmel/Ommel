﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchReturnStatement : XMLLuaSearchElement, IXMLLuaSearchStatement {
		public List<IXMLLuaSearchExpression> Expressions { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

               if (child.Name == "Expressions") {
                    Expressions = new List<IXMLLuaSearchExpression>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[i];

                        var expr = LuaSearchXMLTypes.GetExpression(innerchild.Name);
                        expr.FillIn(innerchild as XmlElement);
                    }
                }
            }
        }
    }
}
