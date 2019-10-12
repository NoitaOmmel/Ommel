using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Ommel {
    public class XMLMerger {
        public XmlDocument Patch;
        public XmlDocument Target;

        public XMLMerger(XmlDocument patch, XmlDocument target) {
            Patch = patch;
            Target = target;
        }

        public XMLMerger(TextReader patch, TextReader target) {
            Patch = new XmlDocument();
            Target = new XmlDocument();

            Target.Load(target);
            Patch.Load(patch);
        }

        public XMLMerger(string patch, string target) {
            Patch = new XmlDocument();
            Target = new XmlDocument();

            using (var w = new StringReader(patch)) {
                using (var xr = XmlReader.Create(w)) {
                    Patch.Load(xr);
                }
            }

            using (var w = new StringReader(target)) {
                using (var xr = XmlReader.Create(w)) {
                    Target.Load(xr);
                }
            }
        }

        protected void MergeElement(XmlDocument d, XmlElement x, XmlElement a, XmlElement b) {
            for (var i = 0; i < a.Attributes.Count; i++) {
                x.SetAttribute(a.Attributes[i].Name, a.Attributes[i].Value);
            }

            for (var i = 0; i < b.Attributes.Count; i++) {
                x.SetAttribute(b.Attributes[i].Name, b.Attributes[i].Value);
            }

            var patch_offs = 0;
            var patch_idx = 0;
            var last_patch_idx = 0;
            var last_target_idx = 0;
            for (var i = 0; i < a.ChildNodes.Count; i++) {
                var node = a.ChildNodes[i];
                patch_idx = i + patch_offs;
                if (patch_idx >= b.ChildNodes.Count) break;

                var patch_node = b.ChildNodes[patch_idx];

                var name = node.Name;
                if (node.Name != patch_node.Name) {
                    patch_offs -= 1;
                    // if we fail to match the node name,
                    // we try to continue with the assumption that a later
                    // node in the original might appear in the patch
                    // and this one is intentionally skipped
                    // (orig could be in the form of [root]/[x,y]/[a,b,c]
                    // while patch could be [root]/[y]/[a,c] and we want this
                    // to work)
                } else last_patch_idx += 1;
                last_target_idx += 1;

                if (node is XmlElement && node.Name == patch_node.Name) {
                    if (((XmlElement)patch_node).HasAttribute("DELETE")) {
                        continue;
                    }
                    var elem = (XmlElement)x.AppendChild(d.CreateElement(((XmlElement)node).Name));
                    MergeElement(d, elem, (XmlElement)node, (XmlElement)patch_node);
                    x.AppendChild(elem);
                }
                else {
                    x.AppendChild(d.ImportNode(node, true));
                }
            }

            for (var i = last_target_idx; i < a.ChildNodes.Count; i++) {
                x.AppendChild(d.ImportNode(a.ChildNodes[i], true));
            }

            for (var i = last_patch_idx; i < b.ChildNodes.Count; i++) {
                if (b.ChildNodes[i].Name == "APPEND") continue;
                x.AppendChild(d.ImportNode(b.ChildNodes[i], true));
            }
        }

        public XmlDocument CreateMergedDocument() {
            var doc = new XmlDocument();

            if (Target.ChildNodes.Count == 0) throw new Exception("Target must have at least a root");
            if (Patch.ChildNodes.Count == 0) return doc;

            var root_elem = doc.CreateElement(Target.ChildNodes[0].Name);
            MergeElement(doc, root_elem, (XmlElement)Target.ChildNodes[0], (XmlElement)Patch.ChildNodes[0]);
            doc.AppendChild(root_elem);

            return doc;
        }

        public static string WriteDocumentToString(XmlDocument doc) {
            using (var w = new StringWriter()) {
                using (var xw = XmlWriter.Create(w)) {
                    doc.WriteTo(xw);
                    xw.Flush();
                }

                return w.GetStringBuilder().ToString();
            }
        }

        public static void WriteDocumentToFile(XmlDocument doc, StreamWriter writer) {
            var settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                NewLineOnAttributes = true,
                OmitXmlDeclaration = true,
            };
            using (var xw = XmlWriter.Create(writer, settings)) {

                doc.WriteTo(xw);
                xw.Flush();
            }
        }
    }
}
