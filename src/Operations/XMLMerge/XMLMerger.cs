﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace Ommel {
    public class XMLMerger {
        /*
         * This used to be a lot more sensible until I noticed how messed up
         * the XMLs in the game are. Therefore, I present to you - the
         * kinda-XHTML/HTML+XML merger that reads in XML files as HTML, merges
         * them with XML files and writes the result as XML. Lord forgive me.
         */       

        public XmlDocument Patch;
        public HtmlDocument Target;

        public XMLMerger(XmlDocument patch, HtmlDocument target) {
            Patch = patch;
            Target = target;
        }

        public XMLMerger(TextReader patch, TextReader target) {
            Patch = new XmlDocument();
            Target = new HtmlDocument();

            Target.Load(target);


            using (var xr = XmlReader.Create(patch, new XmlReaderSettings { CheckCharacters = false })) {
                 Patch.Load(xr);
            }
        }

        public XMLMerger(string patch, string target) {
            Patch = new XmlDocument();
            Target = new HtmlDocument();

            using (var w = new StringReader(patch)) {
                using (var xr = XmlReader.Create(w, new XmlReaderSettings { CheckCharacters = false })) {
                    Patch.Load(xr);
                }
            }

            using (var w = new StringReader(target)) {
                Target.Load(target);
            }
        }

        protected XmlNode HTMLNodeToXMLElement(XmlDocument doc, HtmlNode node) {
            if (node.NodeType == HtmlNodeType.Text) {
                var xtext = doc.CreateTextNode(node.InnerText);
                return xtext;
            }
            if (node.NodeType == HtmlNodeType.Comment) {
                var text = ((HtmlCommentNode)node).Comment;
                // why, HTMLAgilityPack? why?
                text = text.Substring("<!--".Length, text.Length - "<!--".Length - "-->".Length);
                var xcomment = doc.CreateComment(text);
                return xcomment;
            }

            var xnode = doc.CreateElement(node.Name);
            for (var i = 0; i < node.Attributes.Count; i++) {
                var attrib = node.Attributes[i];
                attrib.UseOriginalName = true;
                xnode.SetAttribute(attrib.Name, attrib.Value);
            }
            for (var i = 0; i < node.ChildNodes.Count; i++) {
                var child = node.ChildNodes[i];
                xnode.AppendChild(HTMLNodeToXMLElement(doc, child));
            }
            return xnode;
        }

        protected void MergeElement(XmlDocument d, XmlElement x, HtmlNode a, XmlElement b) {
            for (var i = 0; i < a.Attributes.Count; i++) {
                a.Attributes[i].UseOriginalName = true;
                x.SetAttribute(a.Attributes[i].Name, a.Attributes[i].Value);
            }

            var edited_attrs = new Dictionary<string, string>();

            for (var i = 0; i < b.Attributes.Count; i++) {
                var patch_attrib = b.Attributes[i];
                if (patch_attrib.Name.StartsWith($"CSVAPPEND_", StringComparison.InvariantCulture)) {
                    var target_name = patch_attrib.Name.Substring($"CSVAPPEND_".Length);
                    string target_value;
                    if (!edited_attrs.TryGetValue(target_name, out target_value)) {
                        target_value = a.Attributes[target_name]?.Value;
                    }
                    if (patch_attrib.Value.Length == 0) {
                        throw new Exception("CSVAPPEND_... special attribute must have at least the first character (separator)");
                    }
                    var separator = patch_attrib.Value[0];
                    var patch_value = patch_attrib.Value.Substring(1);

                    string new_value;
                    if (string.IsNullOrWhiteSpace(target_value)) {
                        new_value = patch_value;
                    }
                    else {
                        new_value = target_value + separator + patch_value;
                    }
                    edited_attrs[target_name] = new_value;
                    x.SetAttribute(target_name, new_value);
                } else if (patch_attrib.Name.StartsWith($"CSVREMOVE_", StringComparison.InvariantCulture)) {
                    var target_name = patch_attrib.Name.Substring($"CSVREMOVE_".Length);
                    string target_value;
                    if (!edited_attrs.TryGetValue(target_name, out target_value)) {
                        target_value = a.Attributes[target_name]?.Value;
                    }
                    if (patch_attrib.Value.Length == 0) {
                        throw new Exception("CSVREMOVE_... special attribute must have at least the first character (separator)");
                    }
                    var separator = patch_attrib.Value[0];

                    if (target_value == null) continue;
                    var csv_elem_to_remove = patch_attrib.Value.Substring(1);
                    var csv_index_to_remove = -1;

                    if (csv_elem_to_remove.StartsWith("@", StringComparison.InvariantCulture)) {
                        csv_index_to_remove = int.Parse(csv_elem_to_remove.Substring(1));
                    }

                    var csv_split = target_value.Split(separator);
                    var new_value_builder = new StringBuilder();
                    var matched = false;
                    for (var j = 0; j < csv_split.Length; j++) {
                        var csv_entry = csv_split[j];
                        if (csv_index_to_remove != -1) {
                            if (j == csv_index_to_remove) continue;
                        } else {
                            if (csv_entry == csv_elem_to_remove && !matched) {
                                matched = true;
                                continue;
                            }
                        }
                        new_value_builder.Append(csv_entry);
                        if (j != csv_split.Length - 1) new_value_builder.Append(separator);
                    }

                    var new_value = new_value_builder.ToString();

                    edited_attrs[target_name] = new_value;
                    x.SetAttribute(target_name, new_value);
                } else {
                    x.SetAttribute(b.Attributes[i].Name, b.Attributes[i].Value);
                }
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

                if (node.Name == patch_node.Name) {
                    if (node.NodeType != HtmlNodeType.Element) {
                        x.AppendChild(d.ImportNode(patch_node, true));
                        continue;
                    }

                    if (((XmlElement)patch_node).HasAttribute("DELETE")) {
                        continue;
                    }
                    var attribs = ((XmlElement)patch_node).Attributes;
                    var skip_elem = false;
                    for (var j = 0; j < attribs.Count; j++) {
                        var attrib = attribs[j];
                        if (attrib.Name.StartsWith("MATCH_", StringComparison.InvariantCulture)) {
                            var real_attrib_name = attrib.Name.Substring("MATCH_".Length);

                            if (node.GetAttributeValue(real_attrib_name, null) != attrib.Value) {
                                patch_offs -= 1;
                                last_patch_idx -= 1;
                                skip_elem = true;
                                break;
                            }
                        }
                    }
                    if (skip_elem) {
                        x.AppendChild(HTMLNodeToXMLElement(d, node));
                        continue;
                    }
                    var elem = (XmlElement)x.AppendChild(d.CreateElement(node.Name));
                    MergeElement(d, elem, node, (XmlElement)patch_node);
                    for (var j = 0; j < attribs.Count; j++) {
                        var attrib = attribs[j];
                        if (attrib.Name.StartsWith("MATCH_", StringComparison.InvariantCulture)) {
                            elem.RemoveAttribute(attrib.Name);
                        }
                    }
                    x.AppendChild(elem);
                }
                else {
                    x.AppendChild(HTMLNodeToXMLElement(d, node));
                }
            }

            for (var i = last_target_idx; i < a.ChildNodes.Count; i++) {
                x.AppendChild(HTMLNodeToXMLElement(d, a.ChildNodes[i]));
            }

            for (var i = last_patch_idx; i < b.ChildNodes.Count; i++) {
                if (b.ChildNodes[i].Name == "APPEND") continue;
                if (b is XmlElement && b.ChildNodes[i] is XmlElement && ((XmlElement)b.ChildNodes[i]).HasAttribute("DELETE")) continue;
                x.AppendChild(d.ImportNode(b.ChildNodes[i], true));
            }
        }

        public XmlDocument CreateMergedDocument() {
            var doc = new XmlDocument();



            if (Target.DocumentNode.ChildNodes.Count == 0) throw new Exception("Target must have at least a root");
            if (Patch.ChildNodes.Count == 0) return doc;

            var root_elem = doc.CreateElement(Target.DocumentNode.ChildNodes[0].Name);
            MergeElement(doc, root_elem, Target.DocumentNode.ChildNodes[0], (XmlElement)Patch.ChildNodes[0]);
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
