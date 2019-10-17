using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NetLua;

namespace Ommel {
	public class LuaInsertOperation : FileOperation {
        public static Dictionary<string, XMLLuaSearchElement> LuaSearchElementsByKey = new Dictionary<string, XMLLuaSearchElement>();

        static LuaInsertOperation() {

        }

        public XMLLuaSearchElement LuaInsertTarget;

        private static Parser LuaParser = new Parser();

		public LuaInsertOperation() : base("LuaInsert") {
			WhitelistedFileTypes.Add(Ommel.FileType.Lua);
		}

        public override void CopyTo(FileOperation target) {
            ((LuaInsertOperation)target).LuaInsertTarget = LuaInsertTarget;
        }

        public override void FillInChild(XmlNode node) {
            if (!(node is XmlElement)) return;

            var elem = (XmlElement)node;
            if (elem.Name == "Where") {
                LuaInsertTarget = new XMLLuaSearchBlock();
                LuaInsertTarget.FillIn(elem);
            }
        }

        public override void OnExecute(Ommel loader, Mod mod) {
			loader.RegisterModifiedFile(TargetFile);
			var source_path = mod.GetFile(SourceFile);

            var parsed_script = LuaParser.ParseFile(loader.ExpandTargetPathDefaulted(TargetFile));
			if (parsed_script == null) throw new Exception($"Failed parsing '{TargetFile}'");
			var ast_search = new ASTSearchWalker();
			ast_search.Match(LuaInsertTarget, parsed_script);

			if (!ast_search.HasSelection) throw new Exception($"AST search failed");

			var offset = ast_search.SelectedSpan.Value.Location.Position;

			string s;
            using (var target_file = new StreamReader(File.OpenRead(loader.ExpandTargetPathDefaulted(TargetFile)))) {
                using (var source_file = new StreamReader(File.OpenRead(source_path))) {
                    s = InsertAtPos(target_file, offset, source_file, Trim);
                }
            }

			loader.DeleteFile(TargetFile);

			using (var output = new StreamWriter(loader.OpenWriteTarget(TargetFile))) {
				output.Write(loader.ProcessNewlines(s.ToString()));
			}

			if (loader.ExtraChecks) {
				Logger.Debug($"Checking result to see if it's still valid");
				var parsed_new_script = LuaParser.ParseFile(loader.ExpandTargetPath(TargetFile));
				if (parsed_new_script == null) Logger.Warn($"Insertion broke Lua script");
				else {
					var test_ast_search = new ASTSearchWalker();
					test_ast_search.Match(LuaInsertTarget, parsed_new_script);

					if (!test_ast_search.HasSelection) Logger.Warn($"AST query no longer resolves after insertion");
				}
			}

            DoReplaceIfRequested(mod, loader, TargetFile);
		}
	}
}
