using System;
using System.Xml;

namespace Ommel {
	public interface IXMLLuaSearchExpression {
        void FillIn(XmlElement elem);
    }
}
