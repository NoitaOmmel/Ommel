using System;
using System.IO;

namespace Ommel {
    public class XMLMergeOperation : FileOperation {
        public XMLMergeOperation() : base("XMLMerge") {
            WhitelistedFileTypes.Add(Ommel.FileType.XML);
        }

        public override void OnExecute(Ommel loader, Mod mod) {
            var source_path = mod.GetFile(SourceFile);

            loader.RegisterModifiedFile(TargetFile);

            XMLMerger merger = null;
            using (var source_file = new StreamReader(File.OpenRead(source_path))) { 
                using (var target_file = new StreamReader(File.OpenRead(loader.ExpandTargetPathDefaulted(TargetFile)))) {
                    merger = new XMLMerger(source_file, target_file); // patch, target
                }
            }

            var doc = merger.CreateMergedDocument();

            loader.DeleteFile(TargetFile);
            using (var target_file = new StreamWriter(loader.OpenWriteTarget(TargetFile))) {
                XMLMerger.WriteDocumentToFile(doc, target_file);
            }

            DoReplaceIfRequested(mod, loader, TargetFile);
        }

        public override void ConvertToNoitaAPI(Mod mod, StreamWriter writer) {
            if (TargetFile == "data/magic_numbers.xml" || TargetFile == "data/magic_numbers_disable_debug.xml") {
                writer.WriteLine($"ModMagicNumbersFileAdd(\"{mod.GetAPIPath(SourceFile)}\")");
            }
            else if (TargetFile == "data/materials.xml") {
                writer.WriteLine($"ModMaterialsFileAdd(\"{mod.GetAPIPath(SourceFile)}\")");
            }
            else base.ConvertToNoitaAPI(mod, writer);
        }
    }
}
