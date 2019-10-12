using System;
using System.Xml;

namespace Ommel {
	public abstract class XMLLuaSearchElement {
        public bool Select { get; set; }

        public virtual void FillIn(XmlElement elem) {
            Select = elem.GetAttribute("select") == "1";
        }

    }
}
