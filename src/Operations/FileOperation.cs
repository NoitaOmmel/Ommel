using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ModTheGungeon;

namespace Ommel {
	public abstract class FileOperation {
		public class BlacklistedFileTypeException : Exception {
			public BlacklistedFileTypeException(string name, Ommel.FileType ftype, string why) : base($"You are not allowed to use operation '{name}' on files of type '{ftype}' - {why}") {
			}
		}

		public static Dictionary<string, Type> FileOperationsByKey = new Dictionary<string, Type>();

		private static void AddFileOperation(string key, Type op_type) {
			FileOperationsByKey[key] = op_type;
		}

        public static T CreateFileOperation<T>(string key, Type type) where T : FileOperation {
            var inst = Activator.CreateInstance(type) as T;
            inst.SetKey(key);
            return inst;
        }

        static FileOperation() {
			AddFileOperation("Overwrite", typeof(OverwriteOperation));
            AddFileOperation("LuaInsert", typeof(LuaInsertOperation));
            AddFileOperation("TextInsert", typeof(TextInsertOperation));
            AddFileOperation("Stub", typeof(StubOperation));
            AddFileOperation("Copy", typeof(CopyOperation));
            AddFileOperation("LuaEvent", typeof(LuaEventOperation));
            AddFileOperation("XMLMerge", typeof(XMLMergeOperation));
		}

		public HashSet<Ommel.FileType> BlacklistedFileTypes = new HashSet<Ommel.FileType>();
		public HashSet<Ommel.FileType> WhitelistedFileTypes = new HashSet<Ommel.FileType>();
		public bool TargetFileMustExist = true;
		public string Key = "none";
		public Logger Logger;

        public string SourceFile;

        private string _TargetFile;

        public string PlaceholderFile;
        public List<string> Placeholders;

        public bool Trim;

        public string TargetFile {
            get {
                if (_TargetFile == null) _TargetFile = SourceFile; 
                return _TargetFile = _TargetFile.Replace(Ommel.MOD_ASSETS_NAME + "/", "data/");
            }
            set { _TargetFile = value; }
        }

        public FileOperation() { }
        public FileOperation(string key) { SetKey(key); }

        public void SetKey(string key) {
            Key = key;
			Logger = new Logger($"Operation {Key}");
		}

		protected bool IsBlacklisted(Ommel.FileType type) {
			return (WhitelistedFileTypes.Count > 0 && !WhitelistedFileTypes.Contains(type)) || BlacklistedFileTypes.Contains(type);
		}

		protected void CheckBlacklisted(Ommel.FileType type, string why) {
			//if (IsBlacklisted(type)) throw new BlacklistedFileTypeException(Key, type, why);
		}

		protected string InsertAtPos(TextReader target_file, int offset, TextReader source_file, bool trim = false) {
			var s = new StringBuilder();

			var target_pre_data = new char[offset];
			target_file.Read(target_pre_data, 0, offset);
			s.Append(target_pre_data);

			var data = source_file.ReadToEnd();
			if (trim) data = data.Trim();
			s.Append(data);

			s.Append(target_file.ReadToEnd());

			return s.ToString();
		}

        protected void DoReplaceIfRequested(Mod mod, Ommel loader, string target_path) {
            if (Placeholders == null) return;
            if (Placeholders != null && PlaceholderFile == null) {
                Logger.Warn($"File operation requested to use placeholders, but did not specify where the placeholder file is");
                return;
            }
            var string_map = mod.LoadReplacementMap(PlaceholderFile);
            if (string_map == null && Placeholders != null) {
                Logger.Warn($"File operation requested to use placeholders, but there is no string map file");
                return;
            }

            if (Placeholders != null) mod.ReplaceStringsInFile(string_map, Placeholders.ToArray(), mod.Ommeldata.PlaceholderPrefix, mod.Ommeldata.PlaceholderSuffix, loader.ExpandTargetPath(target_path));
        }

        public void Execute(Ommel loader, Mod mod) {
			Logger.Debug($"Executing");

			if (SourceFile == null) {
				Logger.Error($"No source file ('file') specified");
				return;
			}

			if (!System.IO.File.Exists(mod.GetFile(SourceFile))) {
				Logger.Error($"Specified source file doesn't exist");
				return;
			}
			if (TargetFileMustExist && !System.IO.File.Exists(loader.GetNoitaAssetPath(TargetFile))) {
				Logger.Error($"Specified target file doesn't exist");
				return;
			}

			if (WhitelistedFileTypes.Count > 0) {
				var target_ftype = loader.GetFileTypeFromExtension(TargetFile);
				if (!WhitelistedFileTypes.Contains(target_ftype)) {
					Logger.Error($"File type '{target_ftype}' cannot be used in this operation");
					return;
				}
			} else {
				var target_ftype = loader.GetFileTypeFromExtension(TargetFile);
				if (BlacklistedFileTypes.Contains(target_ftype)) {
					Logger.Error($"File type '{target_ftype}' cannot be used in this operation");
					return;
				}
			}

			OnExecute(loader, mod);
		}

		public abstract void OnExecute(Ommel loader, Mod mod);

        protected string GetXMLTextNode(string context, XmlNode node, int i = 0) {
            if (node.ChildNodes.Count <= i) throw new Exception($"Expected text node for '{context}' in operation '{Key}'");
            node = node.ChildNodes[i];
            if (node is XmlText) return ((XmlText)node).InnerText;
            throw new Exception($"Expected text node for '{context}' in operation '{Key}'");
        }

        public virtual void FillInChild(XmlNode node) {

        }

        public virtual void FillIn(XmlElement elem) {
            Trim = elem.GetAttribute("trim") == "1";
            for (var i = 0; i < elem.ChildNodes.Count; i++) {
                var child = elem.ChildNodes[i];

                switch (child.Name) {
                case "SourceFile": 
                    SourceFile = GetXMLTextNode("SourceFile", child);
                    break;
                case "TargetFile": _TargetFile = GetXMLTextNode("TargetFile", child); break;
                case "Placeholders":
                    Placeholders = new List<string>();
                    PlaceholderFile = (child as XmlElement)?.GetAttribute("file");
                    for (var j = 0; j < child.ChildNodes.Count; j++) {
                        var innerchild = child.ChildNodes[j] as XmlElement;
                        if (innerchild != null && innerchild.Name == "Placeholder" && innerchild.HasAttribute("key")) {
                            Placeholders.Add(innerchild.GetAttribute("key"));
                        }
                    }
                    break;
                default: FillInChild(child); break;
                }
            }
        }

        public virtual void ConvertToNoitaAPI(Mod mod, StreamWriter writer) {
            writer.WriteLine($"-- No conversion for operation {Key}");
        }
    }
}
