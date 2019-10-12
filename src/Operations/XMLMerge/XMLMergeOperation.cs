using System;
using System.IO;

namespace Ommel {
    public class XMLMergeOperation : FileOperation {
        public XMLMergeOperation() : base("XMLMerge") {
            WhitelistedFileTypes.Add(Ommel.FileType.XML);
        }

        public override void OnExecute(Ommel loader, Mod mod) {
            var source_path = mod.GetFile(SourceFile);
            var target_path = loader.GetNoitaAssetPath(TargetFile);

            loader.RegisterModifiedFile(TargetFile);

            XMLMerger merger = null;
            using (var source_file = new StreamReader(File.OpenRead(source_path))) { 
                using (var target_file = new StreamReader(File.OpenRead(target_path))) {
                    merger = new XMLMerger(source_file, target_file); // patch, target
                }
            }

            var doc = merger.CreateMergedDocument();

            File.Delete(target_path);
            using (var target_file = new StreamWriter(File.OpenWrite(target_path))) {
                XMLMerger.WriteDocumentToFile(doc, target_file);
            }

            DoReplaceIfRequested(mod, target_path);
        }
    }
}
