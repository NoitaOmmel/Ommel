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
            if (loader.FileExists(TargetFile)) throw new Exception($"Can't copy as file '{TargetFile}', because it already exists at that path");

			loader.RegisterNewFile(TargetFile);

			loader.DeleteFile(TargetFile);
            loader.CopyFile(mod, SourceFile, TargetFile);

            DoReplaceIfRequested(mod, loader, TargetFile);
		}
	}
}
