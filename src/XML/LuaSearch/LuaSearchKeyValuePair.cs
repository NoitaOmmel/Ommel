using System;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchKeyValuePair {
		public IXMLLuaSearchExpression Key { get; set; }
		public IXMLLuaSearchExpression Value { get; set; }

        public void FillIn(XmlElement elem) {
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Key" && child.ChildNodes.Count > 0) {
                    var key = child.ChildNodes[0];
                    Key = LuaSearchXMLTypes.GetExpression(key.Name, key as XmlElement);
                } else if (child.Name == "Value" && child.ChildNodes.Count > 0) {
                    var value = child.ChildNodes[0];
                    Value = LuaSearchXMLTypes.GetExpression(value.Name, value as XmlElement);
                }
            }
        }
    }
}
