using System;
using System.Xml;
using System.Windows.Forms;

namespace Ommel {
    public class MessageOperation : FileOperation {
        public MessageOperation() : base("Message") {
            TargetFileMustExist = false;
            SourceFileMustExist = false;
            AutoTargetFile = false;
        }

        public string Flag;
        public string Content;

        public override void CopyTo(FileOperation target) {
            ((MessageOperation)target).Flag = Flag;
            ((MessageOperation)target).Content = Content;
        }

        public override void FillInChild(XmlNode node) {
            if (!(node is XmlElement)) return;

            if (((XmlElement)node).Name == "Flag") {
                if (node.ChildNodes.Count < 1 || !(node.ChildNodes[0] is XmlText)) throw new Exception("Flag elements must have a text child");

                Flag = ((XmlText)node.ChildNodes[0]).Value.Trim();
            } else if (((XmlElement)node).Name == "Content") {
                if (node.ChildNodes.Count < 1 || !(node.ChildNodes[0] is XmlText)) throw new Exception("Message elements must have a text child");

                Content = ((XmlText)node.ChildNodes[0]).Value.Trim();
            }
        }

        public override void OnExecute(Ommel loader, Mod mod) {
            if (Flag != null) {
                if (loader.HasFlag(Flag)) {
                    Logger.Debug($"Flag set - avoiding message!");
                    return;
                }
            }

            MessageBox.Show(Content, mod.Metadata.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (Flag != null) loader.Flags.Add(Flag);
        }
    }
}
