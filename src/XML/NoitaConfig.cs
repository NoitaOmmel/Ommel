using System;
using System.Collections.Generic;
using System.Xml;

namespace Ommel {
    public class NoitaConfig {
        public List<string> ModsActive = new List<string>();

        public void FillIn(XmlElement elem) {
            var mods_active = elem.GetAttribute("mods_active");
            if (mods_active != null) {
                ModsActive.AddRange(mods_active.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
