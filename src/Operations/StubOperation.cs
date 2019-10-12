using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ommel {
	public class StubOperation : FileOperation {
		public StubOperation() : base("Stub") {
		}

		public override void OnExecute(Ommel loader, Mod mod) {
			Logger.Debug($"Stubbed.");
		}
	}
}
