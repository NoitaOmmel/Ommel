using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ommel {
    public class BulkOperation : FileOperation {
        public enum BulkMode {
            Permutations,
            OneToOne
        }

        public List<string> SourceFiles;
        public string SourcePattern;
        public List<string> TargetFiles;
        public string TargetPattern;
        public List<Regex> TargetExceptionRules;
        public List<FileOperation> Operations = new List<FileOperation>();
        public BulkMode Mode = BulkMode.Permutations;

        public BulkOperation() : base("Bulk") {
            SourceFileMustExist = false;
            TargetFileMustExist = false;
            AutoTargetFile = false;
        }

        public override void CopyTo(FileOperation target) {
            ((BulkOperation)target).SourceFiles = SourceFiles;
            ((BulkOperation)target).SourcePattern = SourcePattern;
            ((BulkOperation)target).TargetFiles = TargetFiles;
            ((BulkOperation)target).TargetPattern = TargetPattern;
            ((BulkOperation)target).Operations = Operations;
        }

        public override void FillInChild(XmlNode node) {
            if (!(node is XmlElement)) return;

            var child = (XmlElement)node;
            if (child.Name == "SourceFiles") {
                SourceFiles = new List<string>();

                for (var i = 0; i < child.ChildNodes.Count; i++) {
                    var innerchild = child.ChildNodes[i] as XmlElement;
                    if (innerchild == null) continue;

                    if (innerchild.Name != "SourceFile") throw new Exception("Children of SourceFiles must be SourceFile elements");

                    if (innerchild.ChildNodes.Count < 1 || !(innerchild.ChildNodes[0] is XmlText)) throw new Exception("SourceFile elements must have a text child");

                    SourceFiles.Add(((XmlText)innerchild.ChildNodes[0]).Value);
                }
            }
            else if (child.Name == "SourcePattern") {
                if (child.ChildNodes.Count < 1 || !(child.ChildNodes[0] is XmlText)) throw new Exception("SourcePattern elements must have a text child");

                SourcePattern = ((XmlText)child.ChildNodes[0]).Value.Trim();
            } else if (child.Name == "TargetFiles") {
                TargetFiles = new List<string>();

                for (var i = 0; i < child.ChildNodes.Count; i++) {
                    var innerchild = child.ChildNodes[i] as XmlElement;
                    if (innerchild == null) continue;

                    if (innerchild.Name != "TargetFile") throw new Exception("Children of TargetFiles must be TargetFile elements");

                    if (innerchild.ChildNodes.Count < 1 || !(innerchild.ChildNodes[0] is XmlText)) throw new Exception("TargetFile elements must have a text child");

                    TargetFiles.Add(((XmlText)innerchild.ChildNodes[0]).Value);
                }
            } else if (child.Name == "TargetExceptionRules") {
                TargetExceptionRules = new List<Regex>();

                for (var i = 0; i < child.ChildNodes.Count; i++) {
                    var innerchild = child.ChildNodes[i] as XmlElement;
                    if (innerchild == null) continue;

                    if (innerchild.Name != "TargetExceptionRule") throw new Exception("Children of TargetExceptionRules must be TargetExceptionRule elements");

                    if (innerchild.ChildNodes.Count< 1 || !(innerchild.ChildNodes[0] is XmlText)) throw new Exception("TargetExceptionRule elements must have a text child");

                    TargetExceptionRules.Add(new Regex(((XmlText)innerchild.ChildNodes[0]).Value));
                }
            }
            else if (child.Name == "TargetPattern") {
                if (child.ChildNodes.Count < 1 || !(child.ChildNodes[0] is XmlText)) throw new Exception("TargetPattern elements must have a text child");

                TargetPattern = ((XmlText)child.ChildNodes[0]).Value.Trim();
            } else if (child.Name == "Operations") {
                for (var i = 0; i < child.ChildNodes.Count; i++) {
                    var innerchild = child.ChildNodes[i] as XmlElement;
                    if (innerchild == null) continue;

                    Type op_type;
                    if (!FileOperation.FileOperationsByKey.TryGetValue(innerchild.Name, out op_type)) {
                        throw new Exception($"Unknown file operation: {innerchild.Name}");
                    }

                    var op = FileOperation.CreateFileOperation<FileOperation>(innerchild.Name, op_type);
                    op.FillIn(innerchild as XmlElement);

                    Operations.Add(op);
                }
            } else if (child.Name == "Mode") {
                if (child.ChildNodes.Count < 1 || !(child.ChildNodes[0] is XmlText)) throw new Exception("Mode elements must have a text child");

                Mode = (BulkMode)Enum.Parse(typeof(BulkMode), ((XmlText)child.ChildNodes[0]).Value.Trim(), true);
            }
        }

        private List<string> GetSourceFileList(Mod mod) {
            if (SourceFile == null && SourceFiles == null && SourcePattern == null) {
                throw new Exception("You must provide at least one source file for the Bulk operation");
            }

            if (SourceFile != null) return new List<string> { SourceFile };
            if (SourceFiles != null) return SourceFiles;

            var l = new List<string>();

            var regex = new Regex(SourcePattern);
            Logger.Debug($"Source Pattern: {SourcePattern}");

            var source_ents = Directory.GetFiles(mod.DirPath, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < source_ents.Length; i++) {
                var data_ent = source_ents[i].Substring(mod.DirPath.Length + 1);
                if (!regex.IsMatch(data_ent)) continue;
                Logger.Debug($"  Glob match: {data_ent}");

                l.Add(data_ent);
            }

            return l;
        }

        private List<string> GetTargetFileList(Ommel loader) {
            if (TargetFile == null && TargetFiles == null && TargetPattern == null) {
                return new List<string> { "{datapath}" };
            }
            if (TargetFile != null) return new List<string> { TargetFile };
            if (TargetFiles != null) return TargetFiles;

            var l = new List<string>();
            var s = new HashSet<string>();

            var regex = new Regex(TargetPattern);
            Logger.Debug($"Target Pattern: {TargetPattern}");

            var target_ents = Directory.GetFiles(loader.GetTargetPath(), "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < target_ents.Length; i++) {
                var start_idx = loader.GetTargetPath().Length + 1;
                var data_ent = target_ents[i].Substring(start_idx);
                if (!regex.IsMatch(data_ent)) continue;
                var skip = false;
                if (TargetExceptionRules != null) {
                    for (var j = 0; j < TargetExceptionRules.Count; j++) {
                        if (TargetExceptionRules[j].IsMatch(data_ent)) {
                            Logger.Debug($"  Match (target, rejected): {data_ent}");
                            skip = true;
                        }
                    }
                }
                if (skip) continue;
                Logger.Debug($"  Match (target): {data_ent}");
                loader.CheckDataEntry(data_ent);

                l.Add(data_ent);
                s.Add(data_ent);
            }

            var noita_ents = Directory.GetFiles(loader.NoitaPath, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < noita_ents.Length; i++) {
                var start_idx = loader.NoitaPath.Length + 1;
                var data_ent = noita_ents[i].Substring(start_idx);
                if (!regex.IsMatch(data_ent)) continue;

                var skip = false;
                if (TargetExceptionRules != null) {
                    for (var j = 0; j < TargetExceptionRules.Count; j++) {
                        if (TargetExceptionRules[j].IsMatch(data_ent)) {
                            Logger.Debug($"  Match (Noita, rejected): {data_ent}");
                            skip = true;
                        }
                    }
                }
                if (skip) continue;
                if (s.Contains(data_ent)) {
                    Logger.Debug($"  Ignored match: {data_ent}");
                    continue;
                }

                Logger.Debug($"  Glob match (Noita): {data_ent}");

                l.Add(data_ent);
                s.Add(data_ent);
            }

            return l;
        }

        private string ExpandTargetPathVars(string target_path, string source_path) {
            return target_path
                .Replace("{rawsourcepath}", source_path)
                .Replace("{datapath}", source_path.Replace("ommeldata/", "data/"))
                .Replace("{reldatapath}", source_path.Replace("ommeldata/", ""))
                .Replace("{filename}", Path.GetFileName(source_path));
        }

        public override void OnExecute(Ommel loader, Mod mod) {
            var source_file_list = GetSourceFileList(mod);
            var target_file_list = GetTargetFileList(loader);

            if (Mode == BulkMode.OneToOne) {
                if (source_file_list.Count != target_file_list.Count && (target_file_list.Count != 1 || target_file_list[0] != "{datapath}")) {
                    throw new Exception("In OneToOne mode, every source file must have a corresponding target file");
                }
            }

            Logger.Debug($"=== Begin Bulk ===");
            for (var i = 0; i < Operations.Count; i++) {
                var original_op = Operations[i];

                for (var j = 0; j < source_file_list.Count; j++) {
                    var source_file = source_file_list[j];
                    for (var k = 0; k < target_file_list.Count; k++) {
                        if (Mode == BulkMode.OneToOne && k != j) continue;

                        var target_file = target_file_list[k];
                        var new_op = FileOperation.Copy(original_op);

                        new_op.SourceFile = source_file;
                        new_op.TargetFile = ExpandTargetPathVars(target_file, source_file);

                        Logger.Debug($"Source: {source_file}");
                        Logger.Debug($"Target: {new_op.TargetFile}");

                        new_op.Execute(loader, mod);
                    }
                }
            }
            Logger.Debug($"=== End Bulk ===");
        }
    }
}