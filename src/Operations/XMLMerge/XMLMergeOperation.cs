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

            // WHY NOLLA, WHY?!
            if (TargetFile == "data/materials.xml") {
                Logger.Debug($"Applying materials.xml malformed XML workaround");
                string content;
                using (var r = new StreamReader(File.OpenRead(target_path))) {
                    content = r.ReadToEnd();
                }
                content = content.Replace("<!-- ------- REACTIONS ------------ -->", "<!-- REACTIONS -->");
                content = content.Replace("<!--- other reactions ---->", "<!-- other reactions -->");
                content = content.Replace("<!--- [vapour] reactions ---->", "<!--- [vapour] reactions -->");
                content = content.Replace("blob_restrict_to_input_material2=\"1\" \tblob_restrict_to_input_material2=\"1\"", "blob_restrict_to_input_material2=\"1\"");
                content = content.Replace("blob_restrict_to_input_material1=\"1\" \tblob_restrict_to_input_material1=\"1\"", "blob_restrict_to_input_material1=\"1\"");
                content = content.Replace("blob_restrict_to_input_material1=\"1\"\tblob_restrict_to_input_material1=\"1\"", "blob_restrict_to_input_material1=\"1\"");
                content = content.Replace("fire_hp=\"1200000\"", "");
                content = content.Replace("solid_collide_with_self=\"0\"\r\n\tsolid_on_collision_material=\"slime\"\r\n\tsolid_break_to_type=\"slime\"\r\n\tsolid_on_collision_splash_power=\"1\"\r\n\tsolid_collide_with_self=\"0\"", "solid_collide_with_self=\"0\"\r\n\tsolid_on_collision_material=\"slime\"\r\n\tsolid_break_to_type=\"slime\"\r\n\tsolid_on_collision_splash_power=\"1\"");
                content = content.Replace("audio_physics_material_solid=\"metalhollow\"\r\n\taudio_physics_material_solid=\"metalhollow\"", "audio_physics_material_solid=\"metalhollow\"");
                content = content.Replace("temperature_of_fire=\"5\"\r\n\tautoignition_temperature=\"99\"\r\n\ttemperature_of_fire=\"50\"\r\n\tautoignition_temperature=\"85\"", "temperature_of_fire=\"50\"\r\n\tautoignition_temperature=\"85\"");
                content = content.Replace("tags=\"[box2d],[corrodible],[alchemy],[burnable]\"\r\n\tburnable=\"1\"", "tags=\"[box2d],[corrodible],[alchemy],[burnable]\"");
                content = content.Replace("wang_color=\"af3B3B3D\"\r\n\trequires_oxygen=\"0\"\r\n\tliquid_gravity=\"0.3\"", "wang_color=\"af3B3B3D\"\r\n\trequires_oxygen=\"0\"");
                content = content.Replace("wang_color=\"ff3B3B4D\"\r\n\trequires_oxygen=\"0\"\r\n\tliquid_gravity=\"0.3\"", "wang_color=\"ff3B3B4D\"\r\n\trequires_oxygen=\"0\"");
                File.Delete(target_path);
                using (var w = new StreamWriter(File.OpenWrite(target_path))) {
                    w.Write(content);
                }
            }

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

        public override void ConvertToNoitaAPI(Mod mod, StreamWriter writer) {
            if (TargetFile == "data/magic_numbers.xml" || TargetFile == "data/magic_numbers_disable_debug.xml") {
                writer.WriteLine($"ModMagicNumbersFileAdd(\"{mod.GetAPIPath(SourceFile)}\")");
            }
            else base.ConvertToNoitaAPI(mod, writer);
        }
    }
}
