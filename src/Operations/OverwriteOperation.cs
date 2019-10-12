using System;
using System.Collections.Generic;
using System.IO;

namespace Ommel {
	public class OverwriteOperation : FileOperation {
		public OverwriteOperation() : base("Overwrite") {
			BlacklistedFileTypes.Add(Ommel.FileType.XML);
			BlacklistedFileTypes.Add(Ommel.FileType.Lua);
		}

		public override void OnExecute(Ommel loader, Mod mod) {
			loader.RegisterModifiedFile(TargetFile);
			var target_path = loader.GetNoitaAssetPath(TargetFile);

			File.Delete(target_path);
			File.Copy(mod.GetFile(SourceFile), target_path);

            DoReplaceIfRequested(mod, target_path);
        }
	}
}
