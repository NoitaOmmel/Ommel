using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Ommel {
	public struct Mod {
		public const string STRINGS_NAME = "strings.yml";

        public static Dictionary<string, Dictionary<string, string>> ReplacementMapMap = new Dictionary<string, Dictionary<string, string>>();
		public XMLModMetadata Metadata;
        public XMLOmmelMetadata Ommeldata;
		public string DirPath;
		public string DirName;

		public Mod(XMLModMetadata metadata, XMLOmmelMetadata ommeldata, string dir_path) {
			Metadata = metadata;
            Ommeldata = ommeldata;
			DirPath = dir_path;
			DirName = Path.GetFileName(dir_path);
		}

		public string Name {
			get { return Metadata.Name; }
		}

		public string GetFile(string path) {
			return Path.Combine(DirPath, path);
		}

        public string GetAPIPath(string file) {
            return Path.Combine("mods", DirName, file);
        }

        public Dictionary<string, string> LoadReplacementMap(string path) {
            Dictionary<string, string> dict;
            if (ReplacementMapMap.TryGetValue(path, out dict)) return dict;
            if (!Ommeldata.EnableStrings) return null;
            path = GetFile(path);
            if (!File.Exists(path)) return null;
            var doc = new XmlDocument();
            using (var xr = XmlReader.Create(File.OpenRead(path))) {
                doc.Load(xr);
            }

            dict = ReplacementMapMap[path] = new Dictionary<string, string>();

            if (doc.ChildNodes.Count <= 0) throw new Exception($"Invalid format for replacement map");
            var root = doc.ChildNodes[0] as XmlElement;
            if (root == null || root.Name != "Placeholders") throw new Exception($"Replacement map root element must be of type 'Placeholders'");

            for (var i = 0; i < root.ChildNodes.Count; i++) {
                var placeholder = root.ChildNodes[i] as XmlElement;

                if (placeholder == null || placeholder.Name != "Placeholder") throw new Exception($"Replacement map elements must be of type 'Placeholder'");
                var key = placeholder.GetAttribute("key");
                var value = placeholder.GetAttribute("value");
                if (key == null) throw new Exception("Missing key for placeholder");
                if (value == null) throw new Exception($"Missing key for placeholder '{key}'");

                dict[key] = value;
            }
            return dict;
        }

		public string ReplaceStrings(Dictionary<string, string> string_map, string[] keys, string prefix, string suffix, string str) {
            prefix = prefix ?? "";
            suffix = suffix ?? "";
            for (var i = 0; i < keys.Length; i++) {
				var key = keys[i];
				string value = null;
				if (string_map.TryGetValue(key, out value)) {
					str = str.Replace($"{prefix}{key}", value);
				}
				else {
					throw new Exception($"Replacement string '{key}' was mentioned but isn't found in the map");
				}
			}

			return str;
		}

		public void ReplaceStringsInFile(Dictionary<string, string> string_map, string[] keys, string prefix, string suffix, string path) {
			var txt = File.ReadAllText(path);
			txt = ReplaceStrings(string_map, keys, prefix, suffix, txt);
			File.WriteAllText(path, txt);
		}

        public void ConvertToNoitaAPI(StreamWriter writer) {
            for (var i = 0; i < Ommeldata.Operations.Count; i++) {
                var op = Ommeldata.Operations[i];

                op.ConvertToNoitaAPI(this, writer);
            }
        }
    }
}
