using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
	public class XMLLuaSearchTableConstructor : XMLLuaSearchElement, IXMLLuaSearchExpression {
		public List<XMLLuaSearchKeyValuePair> Values { get; set; }

		private Dictionary<IXMLLuaSearchExpression, IXMLLuaSearchExpression> _CachedDict;

		internal Dictionary<IXMLLuaSearchExpression, IXMLLuaSearchExpression> CreateDictionary() {
			if (Values == null) return null;
			if (_CachedDict != null) return _CachedDict;
			_CachedDict = new Dictionary<IXMLLuaSearchExpression, IXMLLuaSearchExpression>();
			for (var i = 0; i < Values.Count; i++) {
				var pair = Values[i];
				_CachedDict[pair.Key] = pair.Value;
			}
			return _CachedDict;
		}

        public override void FillIn(XmlElement elem) {
            base.FillIn(elem);
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                if (child.Name == "Values") {
                    Values = new List<XMLLuaSearchKeyValuePair>();
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[i];

                        if (innerchild.Name != "Entry") continue;

                        var kv = new XMLLuaSearchKeyValuePair();
                        kv.FillIn(innerchild as XmlElement);
                        Values.Add(kv);

                    }
                }
            }
        }
    }
}
