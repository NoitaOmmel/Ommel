using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchFunctionDefinition : XMLLuaSearchElement, IXMLLuaSearchExpression, IXMLLuaSearchStatement {
        public List<string> Arguments { get; set; }
        public XMLLuaSearchBlock Body { get; set; }

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Arguments") {
                    Arguments = new List<string>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];

                        if (innerchild is XmlText) {
                            Arguments.Add(((XmlText)innerchild).InnerText);
                        }
                    }
                } else if (child.Name == "Body" && child.ChildNodes.Count > 0) {
                    var body = child.ChildNodes[0];
                    Body = new XMLLuaSearchBlock();
                    Body.FillIn(body as XmlElement);
                }
            }
        }
    }
}
