using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NetLua;

namespace Ommel {
	public class TextInsertOperation : FileOperation {
        public int? Line;
        public int? Offset;

        public TextInsertOperation() : base("TextInsert") {
			WhitelistedFileTypes.Add(Ommel.FileType.Text);
			WhitelistedFileTypes.Add(Ommel.FileType.Other);
		}

        public override void FillInChild(XmlNode node) {
            if (!(node is XmlElement)) return;

            var elem = (XmlElement)node;
            if (elem.Name == "Line") Line = int.Parse(GetXMLTextNode("Line", elem));
            else if (elem.Name == "Offset") Offset = int.Parse(GetXMLTextNode("Offset", elem));
        }

        public override void OnExecute(Ommel loader, Mod mod) {
			var param_counter = 0;
			if (Line != null) param_counter += 1;
			if (Offset != null) param_counter += 1;
			if (param_counter == 0) throw new Exception($"Missing location parameter - choose either 'Line' or 'Offset'");
			if (param_counter > 1) throw new Exception("Too many parameters given to TextInsert - choose either 'Line' or 'Offset'");

			loader.RegisterModifiedFile(TargetFile);
			var target_path = loader.GetNoitaAssetPath(TargetFile);
			var source_path = mod.GetFile(SourceFile);

			StringBuilder s = new StringBuilder();

			string data_to_be_inserted = null;
			using (var source_file = new StreamReader(File.OpenRead(source_path))) {
				data_to_be_inserted = source_file.ReadToEnd();
				if (Trim) data_to_be_inserted = data_to_be_inserted.Trim();
			}


			var string_map = mod.LoadReplacementMap(PlaceholderFile);
			if (string_map == null && Placeholders != null) {
				Logger.Warn($"File operation requested to use placeholders, but there is no string map file");
				return;
			} else if (string_map != null && Placeholders != null) {
				data_to_be_inserted = mod.ReplaceStrings(string_map, Placeholders.ToArray(), mod.Ommeldata.PlaceholderPrefix, mod.Ommeldata.PlaceholderSuffix, data_to_be_inserted);
			}

			if (Offset != null) {
				using (var target_file = new StreamReader(File.OpenRead(target_path))) {
					var target_pre_data = new char[Offset.Value];
					target_file.Read(target_pre_data, 0, Offset.Value);
					s.Append(target_pre_data);
					s.Append(data_to_be_inserted);
					s.Append(target_file.ReadToEnd());
				}
			} else if (Line != null) {
				using (var target_file = new StreamReader(File.OpenRead(target_path))) {
					var line_idx = 1;
					while (!target_file.EndOfStream) {
						if (line_idx == Line.Value) s.Append(data_to_be_inserted);
						s.AppendLine(target_file.ReadLine());
						line_idx += 1;
					}
				}
			}

			File.Delete(target_path);

			using (var output = new StreamWriter(File.OpenWrite(target_path))) {
				output.Write(loader.ProcessNewlines(s.ToString()));
			}
		}
	}
}
