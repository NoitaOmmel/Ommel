//using System;
//using System.Collections.Generic;

//namespace Ommel {
//    public abstract class XMLLuaSearchArray<T> : IXMLSerializable where T : IXMLSerializable {
//        public List<T> Entries = new List<T>();

//        public abstract string XMLType { get; }

//        public void Deserialize(XMLSerializer ser, XmlElement elem) {
//            for (var i = 0; i < elem.ChildNodes.Count; i++) {
//                Entries.Add(ser.Deserialize<T>(elem.ChildNodes[i]));
//            }
//        }

//        public XmlElement Serialize(XMLSerializer ser, XmlDocument doc) {
//            throw new NotImplementedException();
//        }
//    }

//    public class XMLLuaSearchArrayVariables : XMLLuaSearchArray<IXMLLuaSearchAssignable> {
//        public override string XMLType => "Variables";
//    }

//    public class XMLLuaSearchArrayExpressions : XMLLuaSearchArray<IXMLLuaSearchExpression> {
//        public override string XMLType => "Expressions";
//    }

//    public class XMLLuaSearchArrayArguments : XMLLuaSearchArray<IXMLLuaSearchExpression> {
//        public override string XMLType => "Arguments";
//    }

//    public class XMLLuaSearchArrayVariableNames : XMLLuaSearchArray<XMLLuaSearchVariableName> {
//        public override string XMLType => "VariableNames";
//    }
//}
