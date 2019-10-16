﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Ommel {
	public class LuaEventOperation : FileOperation {
        public string Event;

        public static HashSet<string> ValidEvents = new HashSet<string> {
			"init",
			"enter",
			"leave"
		};

		public LuaEventOperation() : base("LuaEvent") {
			WhitelistedFileTypes.Add(Ommel.FileType.Lua);

			TargetFileMustExist = false;
		}

        public override void FillInChild(XmlNode node) {
            if (!(node is XmlElement)) return;

            var elem = (XmlElement)node;
            if (elem.Name == "Event") Event = GetXMLTextNode("Event", elem);
        }

        public override void OnExecute(Ommel loader, Mod mod) {
			if (!ValidEvents.Contains(Event)) throw new Exception($"Invalid event '{Event}'");

			if (Event == "init") TargetFile = "data/scripts/perks/perk_list.lua"; // lua entry point

			if (!loader.FileExists(TargetFile)) throw new Exception("Target file doesn't exist");

			var event_files = loader.TryGetLuaModEvent(Event);
            var mod_source_file = mod.GetFile(SourceFile);
			if (event_files == null) {
				loader.RegisterModifiedFile(TargetFile);
				event_files = loader.RegisterLuaModEvent(Event);

				var lua_parser = new NetLua.Parser();
				var block = lua_parser.ParseFile(loader.ExpandTargetPathDefaulted(TargetFile));
				NetLua.Ast.IStatement first_non_dofile = null;
				int offset = 0;

				for (var i = 0; i < block.Statements.Count; i++) {
					var statement = block.Statements[i];

					if (statement is NetLua.Ast.FunctionCall) {
						var call = (NetLua.Ast.FunctionCall)statement;
						if (call.Function is NetLua.Ast.Variable) {
							var func = (NetLua.Ast.Variable)call.Function;

							if (func.Name == "dofile") continue;
						}
					}

					first_non_dofile = statement;
					Logger.Debug($"Matched AST");
					break;
				}

				if (first_non_dofile == null) {
					Logger.Warn("Failed to match AST - empty file? Inserting call at the very beginning");
				}
				else offset = first_non_dofile.Span.Location.Position;

				var event_caller = new StringBuilder();
				event_caller.AppendLine("local ommelrt = loadfile(\"data/ommel/ommelrt.lua\")()");
				event_caller.Append("ommelrt.run_event(\"");
				event_caller.Append(Event);
				event_caller.AppendLine($"\", \"{TargetFile}\")");

				string new_target_content = null;

				using (var target_file = new StreamReader(loader.ExpandTargetPathDefaulted(TargetFile))) {
					using (var source_file = new StringReader(event_caller.ToString())) {
						new_target_content = InsertAtPos(target_file, offset, source_file, false);
					}
				}

				loader.DeleteFile(TargetFile);
                using (var f = new StreamWriter(loader.OpenWriteTarget(TargetFile))) {
                    f.Write(new_target_content);
                }
			}

            event_files.Add(SourceFile);

            loader.RegisterNewFile(SourceFile);
            loader.DeleteFile(SourceFile);
            loader.CopyFile(mod, SourceFile, SourceFile);

            DoReplaceIfRequested(mod, loader, TargetFile);
		}

        public override void ConvertToNoitaAPI(Mod mod, StreamWriter writer) {
            if (Event == "leave") {
                writer.WriteLine($"ModLuaFileAppend(\"{TargetFile}\", \"{mod.GetAPIPath(SourceFile)}\")");
            } else {
                base.ConvertToNoitaAPI(mod, writer);
            }
        }
    }
}
