using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetLua;

namespace Ommel {
	public class CopyOperation : FileOperation {
		public CopyOperation() : base("Copy") {
			TargetFileMustExist = false;
		}

		public override void OnExecute(Ommel loader, Mod mod) {
			var target_path = loader.GetNoitaAssetPath(TargetFile);
			var source_path = mod.GetFile(SourceFile);

			var target_dir = Path.GetDirectoryName(TargetFile);
			var target_dirs = target_dir.Split(Path.DirectorySeparatorChar);

			var cur_path = loader.NoitaPath;
			for (var i = 0; i < target_dirs.Length; i++) {
				cur_path += Path.DirectorySeparatorChar + target_dirs[i];
				if (!Directory.Exists(cur_path)) Directory.CreateDirectory(cur_path);
			}

            if (File.Exists(TargetFile)) throw new Exception($"Can't copy as file '{TargetFile}', because a file already exists at that path");

			loader.RegisterNewFile(TargetFile);

			File.Delete(target_path);
			File.Copy(source_path, target_path);

            DoReplaceIfRequested(mod, target_path);
		}
	}
}
