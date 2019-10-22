using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Ommel {
    public class LocalizeOperation : FileOperation {
        public const string TRANSLATION_CSV_FILE = "data/translations/common.csv";

        public LocalizeOperation() : base("Localize") {
            TargetFileMustExist = false;
        }

        private string CSVEscape(string str) {
            if (str.Contains(",") || str.Contains("\"")) str = $"\"{str.Replace("\"", "\"\"")}\"";
            return str;
        }

        private void CSVParseLine(string line, string[] output) {
            var s = new StringBuilder();
            var in_quote = false;
            var elem_idx = 0;
            for (var i = 0; i < line.Length; i++) {
                var c = line[i];
                var n = i + 1 < line.Length ? line[i + 1] : '\0';

                if (c == '"') {
                    if (in_quote && n == '"') {
                        i += 1;
                        s.Append("\"");
                        continue;
                    }
                    in_quote = !in_quote;
                    continue;
                }

                if (c == ',' && !in_quote) {
                    output[elem_idx] = s.ToString();
                    s = new StringBuilder();
                    elem_idx += 1;
                    continue;
                }

                s.Append(c);
            }
            output[elem_idx] = s.ToString();
        }

        public override void OnExecute(Ommel loader, Mod mod) {
            TargetFile = TRANSLATION_CSV_FILE;
            loader.RegisterModifiedFile(TargetFile);

            var available_lang_keys = new Dictionary<string, int>();
            var total_entries = 0;
            var lines_by_key = new Dictionary<string, string[]>();
            var all_keys = new List<string>();
            string header = null;

            using (var reader = new StreamReader(File.OpenRead(loader.ExpandTargetPathDefaulted(TRANSLATION_CSV_FILE)))) {
                var keys_line = reader.ReadLine();
                header = keys_line;

                var keys = keys_line.Split(',');
                total_entries = keys.Length;
                for (var i = 0; i < keys.Length; i++) {
                    var key = keys[i];
                    if (key.Length == 0 || key.StartsWith("NOTES", StringComparison.InvariantCulture)) continue;

                    available_lang_keys[key] = i;
                }


                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var elems = new string[total_entries];

                    CSVParseLine(line, elems);
                    lines_by_key[elems[0]] = elems;
                    all_keys.Add(elems[0]);
                }
            }

            using (var reader = XmlReader.Create(File.OpenRead(mod.GetFile(SourceFile)))) {
                var doc = new XmlDocument();
                doc.Load(reader);

                if (doc.ChildNodes.Count == 0) return;

                var root = doc.ChildNodes[0];
                if (root.Name != "Localization") throw new Exception("Root element for a localization file must be of type Localization");

                for (var i = 0; i < root.ChildNodes.Count; i++) {
                    var child = root.ChildNodes[i];
                    if (!(child is XmlElement)) continue;

                    if (child.Name != "Text" || !((XmlElement)child).HasAttribute("key")) throw new Exception("Children of a Localization element must be Text elements with a key attribute");
                    var str_key = ((XmlElement)child).GetAttribute("key");

                    string[] csv_line;
                    if (!lines_by_key.TryGetValue(str_key, out csv_line)) {
                        lines_by_key[str_key] = csv_line = new string[total_entries];
                        csv_line[0] = CSVEscape(str_key);
                        all_keys.Add(str_key);
                    }

                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j];
                        if (!(innerchild is XmlElement)) continue;

                        if (innerchild.Name != "Language" || !((XmlElement)innerchild).HasAttribute("lang") || !((XmlElement)innerchild).HasAttribute("value")) {
                            throw new Exception("Children of a Text element must be Language elements with lang and value attributes");
                        }

                        var lang_key = ((XmlElement)innerchild).GetAttribute("lang");
                        var lang_idx = 0;
                        if (!available_lang_keys.TryGetValue(lang_key, out lang_idx)) throw new Exception($"Invalid Language key: '{lang_key}'");

                        var lang_value = ((XmlElement)innerchild).GetAttribute("value");

                        csv_line[lang_idx] = lang_value;
                    }
                }
            }

            using (var w = new StreamWriter(loader.OpenWriteTarget(TargetFile))) {
                w.NewLine = "\r\n";
                w.WriteLine(header);
                for (var i = 0; i < all_keys.Count; i++) {
                    var csv_line = lines_by_key[all_keys[i]];
                    for (var j = 0; j < csv_line.Length; j++) {
                        var entry = csv_line[j];
                        if (entry != null) w.Write(CSVEscape(entry));
                        if (j < csv_line.Length - 1) w.Write(",");
                    }
                    w.WriteLine();
                }
            }

            DoReplaceIfRequested(mod, loader, TargetFile);
        }
    }
}
