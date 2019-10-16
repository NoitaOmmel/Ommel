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

			loader.DeleteFile(TargetFile);
			loader.CopyFile(mod, SourceFile, TargetFile);

            DoReplaceIfRequested(mod, loader, TargetFile);
        }
	}
}
