using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;
using ModTheGungeon;

namespace UpdateTool {
    public static class MainClass {
        public static string URL;
        public static string TargetDir;
        public static List<string> FilesToCopy = new List<string>();
        public static Logger Logger = new Logger("UpdateTool");

        public static void DoUpdate(string[] args) {
            for (var i = 0; i < args.Length; i++) {
                var arg = args[i];

                if (arg == "-url") {
                    if (args.Length < i + 1) throw new Exception("Missing -url argument");
                    URL = args[i + 1];
                }
                else if (arg == "-file") {
                    if (args.Length < i + 1) throw new Exception("Missing -file argument");
                    FilesToCopy.Add(args[i + 1]);
                } else if (arg == "-dir") {
                    if (args.Length < i + 1) throw new Exception("Missing -dir argument");
                    TargetDir = args[i + 1];
                }
            }

            if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);

            var tmp_extract_path = Path.Combine(Path.GetTempPath(), "ommel-updatetool");
            Logger.Debug($"Extraction directory: {tmp_extract_path}");
            if (Directory.Exists(tmp_extract_path)) {
                Logger.Debug($"Dir already exists, removing");
                Directory.Delete(tmp_extract_path, true);
            }
            Directory.CreateDirectory(tmp_extract_path);

            var tmp_zip_path = Path.Combine(Path.GetTempPath(), "ommel-updatetool.zip");
            Logger.Debug($"ZIP directory: {tmp_zip_path}");
            if (File.Exists(tmp_zip_path)) File.Delete(tmp_zip_path);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var wc = new System.Net.WebClient()) {
                wc.Headers.Add("User-Agent: request");
                wc.DownloadFile(URL, tmp_zip_path);
            }

            ZipFile.ExtractToDirectory(tmp_zip_path, tmp_extract_path);

            var files = Directory.GetFiles(tmp_extract_path);

            for (var i = 0; i < files.Length; i++) {
                var extract_path = files[i];
                var target_path = Path.Combine(TargetDir, Path.GetFileName(extract_path));

                // wtf windows? moving a used file is A-OK but deleting it is bad
                var bkp_path = target_path + ".bkp";
                if (File.Exists(bkp_path)) File.Delete(bkp_path);
                if (File.Exists(target_path)) File.Move(target_path, bkp_path);
                File.Copy(extract_path, target_path);
            }

            Logger.Debug($"Done");
            return;
        }

        public static void Main(string[] args) {
            try {
                DoUpdate(args);
            } catch (Exception e) {
                MessageBox.Show($"Unexpected error occured while updating:\n{e.Message}", "UpdateTool Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
