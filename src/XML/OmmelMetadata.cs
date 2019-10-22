using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Ommel {
    [XmlRoot("OmmelMod", Namespace = "http://namespace/file.xsd")]
    public class XMLOmmelMetadata {
        [XmlAttribute("enable_strings")]
        public bool EnableStrings;

        [XmlAttribute("placeholder_prefix")]
        public string PlaceholderPrefix;

        [XmlAttribute("placeholder_suffix")]
        public string PlaceholderSuffix;

        public List<FileOperation> Operations { get; set; }

        public void FillIn(XmlElement elem) {
            EnableStrings = elem.GetAttribute("strings") == "1";
            PlaceholderPrefix = elem.GetAttribute("placeholder_prefix");
            PlaceholderSuffix = elem.GetAttribute("placeholder_suffix");

            Operations = new List<FileOperation>();
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];
                if (!(child is XmlElement)) continue;

                Type op_type;
                if (!FileOperation.FileOperationsByKey.TryGetValue(child.Name, out op_type)) {
                    throw new Exception($"Unknown file operation: {child.Name}");
                }

                var op = FileOperation.CreateFileOperation<FileOperation>(child.Name, op_type);
                op.FillIn(child as XmlElement);

                Operations.Add(op);
            }
        }
    }
}
