using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ModTheGungeon;
using NetLua;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace Ommel {
	public static class OmmelMain {
		public static void Main(string[] args) {
			var loader = new Ommel();
			loader.InterpretArgs(args);
			loader.Start(args);
			return;
		}
	}

	public class Ommel {
#if DEBUG
        public const string VERSION = "0.1.10-dev";
#else
        public const string VERSION = "0.1.10";
#endif
        public const string NOITA_VERSION = "mods-beta 1+";
		public const string MODS_FOLDER_NAME = "mods";
		public const string MOD_METADATA_NAME = "mod.xml";
        public const string MOD_OMMELDATA_NAME = "ommel.xml";
		public const string MOD_ASSETS_NAME = "ommeldata";
		public const string BACKUP_FOLDER_NAME = ".ommel-backup";
		public const string BACKUP_FILE_INFO_NAME = ".ommel-files";
        public const string OMMEL_LOG_NAME = "ommel-log.txt";
        public static readonly string[] OMMEL_FILES = {
            "Irony.dll",
            "Irony.xml",
            "Logger.dll",
            "Logger.pdb",
            "NetLua.dll",
            "NetLua.pdb",
            "Ommel.exe",
            "Ommel.pdb",
        };


        public static Logger Logger = new Logger("Ommel");

		static Ommel() {
            Logger.MaxLogLevel = Logger.LogLevel.Debug;
		}

		public string NoitaPath;
		public string NoitaModsPath;
        public string NoitaAppDataPath;
        public string NoitaSaveConfigPath;
        public string NoitaOmmelBackupPath;
		public string NoitaOmmelBackupFileInfoPath;
		public string NoitaOmmelDataPath;
		public string NoitaOmmelStaticDataPath;
		public string NoitaOmmelLibDataPath;
        public string NoitaOmmelExtractInfoPath;
        public string NoitaOmmelLogPath;
        public string NoitaDataWakPath;
        public string NoitaLaunchExe;
        public string NoitaLaunchArgs;
        public List<Mod> Mods;
		public bool ExtraChecks = false;
        public bool IgnoreModLoadOrder = false;
        public bool LaunchNoitaAfterwards = true;
        public bool WriteAutoAPIConversion = false;

        private bool CurrentlyStitchingMods;
        private string LastStitchedMod;
        private StreamWriter LogFile;
		private List<string> BackedUpFiles = new List<string>();
		private List<string> AddedFiles = new List<string>();
		private Dictionary<string, List<string>> ModEvents = new Dictionary<string, List<string>>();
        private List<string> Errors;

		public enum FileType {
			PNG,
			Lua,
			XML,
			Text,
			Other
		}

		public FileType GetFileTypeFromExtension(string str) {
			switch (str.Substring(str.Length - 4, 4)) {
			case ".png": return FileType.PNG;
			case ".lua": return FileType.Lua;
			case ".xml": return FileType.XML;
			case ".txt": return FileType.Text;
			default: return FileType.Other;
			}
		}

		public Ommel() {
            Logger.Subscribe((logger, loglevel, indent, str) => {
                var log_str = logger.String(loglevel, str, indent);
                if (LogFile != null) {
                    LogFile.WriteLine(log_str);
                }
                if (loglevel == Logger.LogLevel.Error) {
                    if (CurrentlyStitchingMods) log_str = $"[mod {LastStitchedMod}] {log_str}";
                    Errors.Add(log_str);
                }
            });
		}

		private void SetGamePaths(string noita_path, string noita_appdata_path = null) {
            if (noita_path == null) 
			if (!Directory.Exists(noita_path)) throw new Exception($"Directory '{noita_path}' doesn't exist!");
			NoitaPath = noita_path;

			NoitaModsPath = Path.Combine(NoitaPath, MODS_FOLDER_NAME);
			if (!Directory.Exists(NoitaModsPath)) Directory.CreateDirectory(NoitaModsPath);

			NoitaOmmelBackupPath = Path.Combine(NoitaPath, BACKUP_FOLDER_NAME);
			if (!Directory.Exists(NoitaOmmelBackupPath)) Directory.CreateDirectory(NoitaOmmelBackupPath);

			NoitaOmmelBackupFileInfoPath = Path.Combine(NoitaPath, BACKUP_FILE_INFO_NAME);
			NoitaOmmelDataPath = Path.Combine(NoitaPath, "data/ommel");
			if (!Directory.Exists(NoitaOmmelDataPath)) Directory.CreateDirectory(NoitaOmmelDataPath);
			NoitaOmmelStaticDataPath = Path.Combine(NoitaPath, "data/ommel/static.lua");
			NoitaOmmelLibDataPath = Path.Combine(NoitaPath, "data/ommel/ommelrt.lua");

            NoitaOmmelExtractInfoPath = Path.Combine(NoitaPath, "data", "extractinfo.txt");
            NoitaDataWakPath = Path.Combine(NoitaPath, "data", "data.wak");

            NoitaOmmelLogPath = Path.Combine(NoitaPath, OMMEL_LOG_NAME);
        }

        private void SetAppdataPaths(string noita_appdata_path) {
            NoitaAppDataPath = noita_appdata_path;
            NoitaSaveConfigPath = Path.Combine(NoitaAppDataPath, "save_shared", "config.xml");
        }

		private void VerifyPaths() {
			if (NoitaPath == null) throw new Exception("Noita path wasn't set - please use -noita-path <path>");
            if (NoitaAppDataPath == null) throw new Exception("Noita appdata path wasn't set - please use -noita-appdata-path <path>");
        }

		private void LoadMod(string path) {
            try {

                var filename = Path.GetFileName(path);

                var metadata_path = Path.Combine(path, MOD_METADATA_NAME);
                if (!File.Exists(metadata_path)) {
                    Logger.Warn($"Ignoring mod entry '{filename}' (missing {MOD_METADATA_NAME})");
                    return;
                }

                var ommeldata_path = Path.Combine(path, MOD_OMMELDATA_NAME);
                if (!File.Exists(ommeldata_path)) {
                    Logger.Warn($"Ignoring mod entry '{filename}' (missing {MOD_OMMELDATA_NAME})");
                    return;
                }

                var metadata = new XMLModMetadata();
                using (var f = XmlReader.Create(File.OpenRead(metadata_path), new XmlReaderSettings { CheckCharacters = false })) {
                    var doc = new XmlDocument();
                    doc.Load(f);
                    if (doc.ChildNodes.Count < 1) metadata = null;
                    else metadata.FillIn(doc.ChildNodes[0] as XmlElement);
                }
                if (metadata == null) {
                    Logger.Error($"Ignoring mod entry '{filename}' (failed to load metadata)");
                    return;
                }

                if (metadata.Name == null) metadata.Name = "Unknown";

                var ommeldata = new XMLOmmelMetadata();
                using (var f = XmlReader.Create(File.OpenRead(ommeldata_path), new XmlReaderSettings { CheckCharacters = false })) {
                    var doc = new XmlDocument();
                    doc.Load(f);
                    if (doc.ChildNodes.Count < 1) ommeldata = null;
                    else ommeldata.FillIn(doc.ChildNodes[0] as XmlElement);
                }
                if (ommeldata == null) {
                    Logger.Error($"Ignoring mod entry '{filename}' (failed to load ommeldata)");
                    return;
                }

                if (metadata.Name == null) metadata.Name = "Unknown";


                if (ommeldata.Operations == null) {
                    Logger.Error($"Ignoring mod entry '{filename}' (no file operations defined)");
                    return;
                }

                Logger.Info($"Found valid mod: '{metadata.Name}'");

                Mods.Add(new Mod(metadata, ommeldata, path));
            } catch (Exception e) {
                Logger.Error($"Failed loading mod '{Path.GetFileName(path)}': {e.Message}");
            }
        }

		private void LoadMods() {
			Mods = new List<Mod>();

            if (IgnoreModLoadOrder) {
                var ents = Directory.GetFileSystemEntries(NoitaModsPath);
                for (var i = 0; i < ents.Length; i++) {
                    var path = ents[i];
                    var filename = Path.GetFileName(path);
                    if (!Directory.Exists(path)) {
                        Logger.Warn($"Ignoring mod entry '{filename}' (not a directory)");
                        continue;
                    }

                    LoadMod(path);
                }
            } else {
                var conf = LoadSaveConfig();
                for (var i = 0; i < conf.ModsActive.Count; i++) {
                    var mod = conf.ModsActive[i];
                    var path = Path.Combine(NoitaModsPath, mod);
                    if (!Directory.Exists(path)) {
                        Logger.Warn($"Ignoring mod entry '{mod}' (set in load order but doesn't exist)");
                        continue;
                    }

                    LoadMod(path);
                }
            }
        }

		private void ExecuteOperation(Mod mod, FileOperation op) {
			op.Execute(this, mod);
		}

		private void StitchMod(Mod mod) {
			Logger.Info($"Stitching mod: '{mod.Name}' ({mod.Ommeldata.Operations.Count} operation(s))");

            LastStitchedMod = mod.DirName;

			for (var i = 0; i < mod.Ommeldata.Operations.Count; i++) {
				var op = mod.Ommeldata.Operations[i];

				ExecuteOperation(mod, op);
			}
		}

		private void StitchMods() {
            CurrentlyStitchingMods = true;
			if (Mods.Count == 0) Logger.Warn($"No valid mods found");
			for (var i = 0; i < Mods.Count; i++) {
				var mod = Mods[i];

				try {
					StitchMod(mod);
				} catch (Exception e) {
					Logger.Error($"Stitching mod '{mod.Name}' failed - game assets might be corrupted");
					Logger.ErrorPretty(e);
					return;
				}
			}
            CurrentlyStitchingMods = false;
			WriteOmmelData();
		}

		private void WriteFileInfo() {
			using (var f = new StreamWriter(File.OpenWrite(NoitaOmmelBackupFileInfoPath))) {
				f.WriteLine($"#backup");
				for (var i = 0; i < BackedUpFiles.Count; i++) {
					f.WriteLine(BackedUpFiles[i]);
				}
				f.WriteLine($"#new");
				for (var i = 0; i < AddedFiles.Count; i++) {
					f.WriteLine(AddedFiles[i]);
				}
			}
		}

		public List<string> TryGetLuaModEvent(string ev) {
			List<string> test;
			if (ModEvents.TryGetValue(ev, out test)) return test;
			return null;
		}

		public List<string> RegisterLuaModEvent(string ev) {
			var existing_ev = TryGetLuaModEvent(ev);
			if (existing_ev != null) return existing_ev;
			var list = new List<string>();
			ModEvents[ev] = list;
			return list;
		}

		private void WriteOmmelData() {
            var ommel_data_path = Path.Combine(NoitaPath, "data", "ommel");
            if (!Directory.Exists(ommel_data_path)) {
                Directory.CreateDirectory(ommel_data_path);
            }
            WriteStaticData();
			WriteLib();
		}

		private void WriteStaticData() {
			RegisterNewFile("data/ommel/static.lua");
			using (var f = new StreamWriter(File.OpenWrite(NoitaOmmelStaticDataPath))) {
				f.WriteLine("local static = {");
				f.WriteLine("\tPATH_SEPARATOR = \"/\",");
				f.WriteLine("\tEVENT_ROOT = \"data/ommel/event/\",");
				f.WriteLine("\tEVENTS = {}");
				f.WriteLine("}");

				foreach (var ev in ModEvents) {
					f.WriteLine($"static.EVENTS[\"{ev.Key}\"] = {{");
					for (var i = 0; i < ev.Value.Count; i++) {
						f.WriteLine($"\t\"{ev.Value[i]}\",");
					}
					f.WriteLine($"}}");
				}

				f.WriteLine("return static");
			}
		}

		private void WriteLib() {
			RegisterNewFile("data/ommel/ommelrt.lua");
			using (var f = new StreamWriter(File.OpenWrite(NoitaOmmelLibDataPath))) {
				using (var s = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ommelrt.lua"))) {
					f.Write(s.ReadToEnd());
				}
			}
		}

        private NoitaConfig LoadSaveConfig() {
            var conf = new NoitaConfig();
            if (!File.Exists(NoitaSaveConfigPath)) {
                Logger.Warn($"Save config doesn't exist ('{NoitaSaveConfigPath}', using defaults)");
                return conf;
            }
            var doc = new XmlDocument();
            using (var xr = XmlReader.Create(File.OpenRead(NoitaSaveConfigPath), new XmlReaderSettings { CheckCharacters = false })) {
                doc.Load(xr);
            }
            conf.FillIn(doc.ChildNodes[0] as XmlElement);
            return conf;
        }

        private void CheckIfUpdated() {
            if (File.Exists(NoitaDataWakPath)) {
                Logger.Info($"Data.wak exists - assuming the game updated! (or first time running)");
                ClearBackup();
                DeleteWakFiles();
                ExtractWakFiles();
            } else {
                if (File.Exists(NoitaOmmelExtractInfoPath)) {
                    Logger.Debug($"Data extracted using Ommel (which means it's tracked right) - OK.");
                } else {
                    Logger.Warn($"Data.wak was extracted using different tool - files aren't properly tracked.");
                }
            }
        }

        private void ExtractWakFiles() {
            Logger.Info($"Extracting files - this may take a couple of seconds");

            var files = WakExtractor.Extract(NoitaDataWakPath, NoitaPath);
            using (var s = new StreamWriter(File.OpenWrite(NoitaOmmelExtractInfoPath))) {
                for (var i = 0; i < files.Count; i++) {
                    var file = files[i];
                    s.WriteLine(file);
                }
            }
            File.Delete(NoitaDataWakPath);
        }

        private void DeleteWakFiles() {
            if (File.Exists(NoitaOmmelExtractInfoPath)) {
                Logger.Info($"Restoring data folder");

                using (var f = new StreamReader(File.OpenRead(NoitaOmmelExtractInfoPath))) {
                    while (!f.EndOfStream) {
                        var wak_file = f.ReadLine();
                        var wak_file_path = Path.Combine(NoitaPath, wak_file);
                        Logger.Debug($"Deleting tracked wak file: {wak_file}");
                    }
                }
                File.Delete(NoitaOmmelExtractInfoPath);
            }
        }

        private void ErrorMessageBox(string content) {
            MessageBox.Show(content, "Ommel Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetLatestVersion() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string content = null;
            using (var wc = new System.Net.WebClient()) {
                wc.Headers.Add("User-Agent: request");
                content = wc.DownloadString("https://api.github.com/repos/NoitaOmmel/Ommel/releases/latest");
            }

            var pat = new Regex("\"tag_name\":\"v(?<tag>[^\"]+)\"");
            var match = pat.Match(content);
            return match.Groups["tag"].Value;
        }

        private string EscapeArgument(string arg) {
            return arg.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private void RunAutoUpdate(string[] cmd_args, string url) {
            Logger.Debug($"Starting UpdateTool");

            var proc = new ProcessStartInfo();
            proc.UseShellExecute = false;
            proc.Arguments = "";
            var update_tool_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "UpdateTool.exe");
            if (Environment.OSVersion.Platform.HasFlag(PlatformID.Unix)) {
                proc.FileName = "mono";
                proc.Arguments += $"\"{update_tool_path}\" ";
            } else {
                proc.FileName = update_tool_path;
            }

            var files = new StringBuilder();
            for (var i = 0; i < OMMEL_FILES.Length; i++) {
                files.Append($"-file \"{OMMEL_FILES[i]}\" ");
            }

            
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var args = $"-url \"{url}\" -dir \"{dir}\" {files.ToString()}";

            Logger.Debug($"Arguments: {args}");

            proc.Arguments += args;
            var running_proc = Process.Start(proc);
            running_proc.WaitForExit();
            if (running_proc.ExitCode == 0) {
                MessageBox.Show("The update was a success! Launch Ommel again to use it.", "Ommel Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                throw new Exception("Failed auto-updating");
            }
            Environment.Exit(0);
        }

        private bool CheckIfOutdated(string[] cmd_args) {
            try {
                var latest_ver = GetLatestVersion();
                Logger.Debug($"Latest available Ommel version: {latest_ver}");
                if (latest_ver != VERSION.Replace("-dev", "")) {
                    Logger.Debug($"Ommel is out of date");
                    if (MessageBox.Show($"Ommel v{latest_ver} is now available!\nIf you press \"OK\", the program will download and install the update automatically.\nYou will have to start the game again to run the new version.", "Ommel Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
                        var url = $"https://github.com/NoitaOmmel/Ommel/releases/download/v{latest_ver}/Ommel-v{latest_ver}.zip";
                        RunAutoUpdate(cmd_args, url);
                    }
                    return true;
                }
            } catch (Exception e) {
                Logger.Warn($"Failed obtaining latest release: {e}");
            }
            return false;
        }

        public string ProcessNewlines(string data) {
			if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
				return data.Replace("\r", "");
			}
			return data;
		}

		public void InterpretArgs(string[] args) {
            for (var i = 0; i < args.Length; i++) {
                var arg = args[i];

                if (arg == "-noita-path") {
                    if (args.Length <= i + 1) throw new Exception("Missing path after -noita-path");

                    SetGamePaths(args[i + 1]); 
                } else if (arg == "-noita-appdata-path") {
                    if (args.Length <= i + 1) throw new Exception("Missing path after -noita-appdata-path");

                    SetAppdataPaths(args[i + 1]); 
                } else if (arg == "-extra-checks") {
					ExtraChecks = true;
				} else if (arg == "-exe") {
                    if (args.Length <= i + 1) throw new Exception("Missing executable after -exe");

                    NoitaLaunchExe = args[i + 1];
                } else if (arg == "-args") {
                    if (args.Length <= i + 1) throw new Exception("Missing args after -args");

                    NoitaLaunchArgs = args[i + 1];
                } else if (arg == "-dont-launch") {
                    LaunchNoitaAfterwards = false;
                } else if (arg == "-write-auto-api-conversion") {
                    WriteAutoAPIConversion = true;
                }
            }
		}

        public bool IsModDataEntry(string path) {
            return path.StartsWith(MOD_ASSETS_NAME + "/", StringComparison.InvariantCulture);
        }

        public bool IsDataEntry(string path) {
			return path.StartsWith("data/", StringComparison.InvariantCulture);
		}

		public void CheckDataEntry(string path) {
			if (!IsDataEntry(path)) throw new Exception($"'{path}' isn't a valid data/ path");
		}

        public void CheckModDataEntry(string path) {
            if (!IsModDataEntry(path)) throw new Exception($"'{path}' isn't a valid ommeldata/ path");
        }

		public void RegisterNewFile(string path) {
			CheckDataEntry(path);

			AddedFiles.Add(path);
		}

		public string RegisterModifiedFile(string data_path) {
			var path = GetNoitaAssetPath(data_path);

			BackedUpFiles.Add(data_path);
			var backup_path = Path.Combine(NoitaOmmelBackupPath, data_path);
			if (File.Exists(backup_path)) {
				Logger.Debug($"Modified file '{data_path}' already backed up");
				return backup_path;
			}

			if (Directory.Exists(path)) throw new Exception("Used BackupFile on a directory");
			if (!File.Exists(path)) throw new FileNotFoundException($"Failed to backup file '{data_path}' - it doesn't exist");

			var dirs = Path.GetDirectoryName(data_path).Split('/');
			string cur_path = NoitaOmmelBackupPath;
			for (var i = 0; i < dirs.Length; i++) {
				var dir = dirs[i];
				cur_path += Path.DirectorySeparatorChar;
				cur_path += dir;

				if (!Directory.Exists(cur_path)) Directory.CreateDirectory(cur_path);
			}

			File.Copy(path, backup_path);

			return backup_path;
		}

		public string GetNoitaAssetPath(string path) {
			CheckDataEntry(path);

			return Path.Combine(NoitaPath, path);
		}

		public void TryRestoreData() {
			if (File.Exists(NoitaOmmelBackupFileInfoPath)) {
				Logger.Debug($"Restoring data folder");

				using (var f = new StreamReader(File.OpenRead(NoitaOmmelBackupFileInfoPath))) {
					string op = null;
					while (!f.EndOfStream) {
						var l = f.ReadLine();
                        if (l == "#backup") op = "#backup";
                        else if (l == "#new") op = "#new";
                        else if (l == "#wak") op = "#wak";
                        else {
                            if (op == null) throw new Exception($"Corrupted {BACKUP_FILE_INFO_NAME} file");

                            var target_path = Path.Combine(NoitaPath, l);

                            if (op == "#backup") {
                                var source_path = Path.Combine(NoitaOmmelBackupPath, l);
                                if (!File.Exists(source_path)) {
                                    Logger.Error($"Failed restore of '{target_path}' - wasn't backed up!");
                                }
                                if (File.Exists(target_path)) File.Delete(target_path);
                                File.Copy(source_path, target_path);
                                Logger.Debug($"Restored '{l}'");
                            }
                            else if (op == "#new") {
                                if (File.Exists(target_path)) File.Delete(target_path);
                                Logger.Debug($"Deleted '{l}'");
                            }
                            else if (op == "#wak") {
                                // do nothing
                            }
                            else {
                                throw new Exception($"Unknown op: {op}");
                            }
                        }
					}
				}

				File.Delete(NoitaOmmelBackupFileInfoPath);
			}
		}

        public void ClearBackup() {
            File.Delete(NoitaOmmelBackupFileInfoPath);
            Directory.Delete(NoitaOmmelBackupPath, true);
        }

        public void ConvertToNoitaAPI() {
            if (!WriteAutoAPIConversion) return;
            for (var i = 0; i < Mods.Count; i++) {
                var mod = Mods[i];
                using (var w = new StreamWriter(mod.GetFile("ommel-autoconvert.lua")))
                    mod.ConvertToNoitaAPI(w);
            }
        }

        public void LaunchNoita() {
            if (!LaunchNoitaAfterwards) return;
            Logger.Info($"Starting Noita");
            var proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = NoitaPath;
            proc.FileName = NoitaLaunchExe;
            proc.Arguments = NoitaLaunchArgs;
            Logger.Debug($"Exe: '{NoitaLaunchExe}' Arguments: '{NoitaLaunchArgs}'");
            var proc_running = Process.Start(proc); 
            proc_running.WaitForExit();
            if (proc_running.ExitCode != 0) {
                throw new Exception($"Failed launching Noita!");
            }

        }

        public void Start(string[] cmd_args) {
            Console.CancelKeyPress += (sender, e) => {
                if (LogFile != null) LogFile.Dispose();
            };

            Errors = new List<string>();

            Logger.Info($"OMMEL v{VERSION} STARTING");
			Logger.Info($"Target: Noita {NOITA_VERSION}");

            if (NoitaPath == null && NoitaLaunchExe != null) {
                if (File.Exists(NoitaLaunchExe)) {
                    NoitaPath = Path.GetDirectoryName(NoitaLaunchExe);
                }
            }

            if (NoitaLaunchExe == null && NoitaPath != null) {
                NoitaLaunchExe = Path.Combine(NoitaPath, "noita.exe");
            }

            if (NoitaLaunchArgs == null) NoitaLaunchArgs = "";

            if (NoitaAppDataPath == null) {
                SetAppdataPaths(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Nolla_Games_Noita"));
            }

            CheckIfOutdated(cmd_args);

            File.Delete(NoitaOmmelLogPath);
            LogFile = new StreamWriter(File.OpenWrite(NoitaOmmelLogPath));

            try {
                CheckIfUpdated();
                TryRestoreData();
                VerifyPaths();
                LoadMods();
                ConvertToNoitaAPI();
                StitchMods();
                WriteFileInfo();
                LaunchNoita();
            } catch (Exception e) {
                ErrorMessageBox($"Ommel encountered an error that caused it to terminate:\n{e.Message}");
            }
            finally {
                if (Errors.Count > 0) {
                    var s = new StringBuilder();
                    for (var i = 0; i < Errors.Count; i++) {
                        s.AppendLine(Errors[i]);
                    }
                    ErrorMessageBox($"Ommel has encountered at least one error while trying to stitch mods:\n{s.ToString()}");
                }
                LogFile.Dispose();
                LogFile = null;
            }
		}
	}
}
