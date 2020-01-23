using System;
using System.Collections.Generic;
using System.Xml;

namespace Ommel {
    public class NoitaModConfig {
        public List<string> Mods = new List<string>();
        public List<string> EnabledMods = new List<string>();

        public void FillIn(XmlElement elem) {
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i] as XmlElement;
                if (child == null) continue;
                var name = child.GetAttribute("name");
                Mods.Add(name);
                if (child.GetAttribute("enabled") == "1") {
                    EnabledMods.Add(name);
                }
            }
        }
    }
}
