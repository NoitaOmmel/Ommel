using System;
using System.Collections.Generic;
using System.Xml;

namespace Ommel {
    public static class LuaSearchXMLTypes {
        public static Dictionary<string, Type> Assignables = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Statements = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Expressions = new Dictionary<string, Type>();

        static LuaSearchXMLTypes() {
            Assignables.Add("Variable", typeof(XMLLuaSearchVariable));
            Assignables.Add("TableAccess", typeof(XMLLuaSearchTableAccess));

            Statements.Add("GenericFor", typeof(XMLLuaSearchGenericFor));
            Statements.Add("BreakStatement", typeof(XMLLuaSearchBreakStatement));
            Statements.Add("FunctionCall", typeof(XMLLuaSearchFunctionCall));
            Statements.Add("Block", typeof(XMLLuaSearchBlock));
            Statements.Add("FunctionDefinition", typeof(XMLLuaSearchFunctionDefinition));
            Statements.Add("RepeatStatement", typeof(XMLLuaSearchRepeatStatement));
            Statements.Add("LocalAssignment", typeof(XMLLuaSearchLocalAssignment));
            Statements.Add("ReturnStatement", typeof(XMLLuaSearchReturnStatement));
            Statements.Add("NumericFor", typeof(XMLLuaSearchNumericFor));
            Statements.Add("IfStatement", typeof(XMLLuaSearchIfStatement));
            Statements.Add("Assignment", typeof(XMLLuaSearchAssignment));
            Statements.Add("WhileStatement", typeof(XMLLuaSearchWhileStatement));

            Expressions.Add("TableConstructor", typeof(XMLLuaSearchTableConstructor));
            Expressions.Add("NilLiteral", typeof(XMLLuaSearchNilLiteral));
            Expressions.Add("FunctionCall", typeof(XMLLuaSearchFunctionCall));
            Expressions.Add("FunctionDefinition", typeof(XMLLuaSearchFunctionDefinition));
            Expressions.Add("BoolLiteral", typeof(XMLLuaSearchBoolLiteral));
            Expressions.Add("NumberLiteral", typeof(XMLLuaSearchNumberLiteral));
            Expressions.Add("BinaryExpression", typeof(XMLLuaSearchBinaryExpression));
            Expressions.Add("VarargsLiteral", typeof(XMLLuaSearchVarargsLiteral));
            Expressions.Add("TableAccess", typeof(XMLLuaSearchTableAccess));
            Expressions.Add("StringLiteral", typeof(XMLLuaSearchStringLiteral));
            Expressions.Add("UnaryExpression", typeof(XMLLuaSearchUnaryExpression));
            Expressions.Add("Variable", typeof(XMLLuaSearchVariable));
        }

        public static IXMLLuaSearchAssignable TryGetAssignable(string key, XmlElement content = null) {
            Type type;
            Assignables.TryGetValue(key, out type);
            if (type == null) return null;
            var inst = Activator.CreateInstance(type) as IXMLLuaSearchAssignable;
            if (content != null) inst.FillIn(content);
            return inst;
        }

        public static IXMLLuaSearchAssignable GetAssignable(string key, XmlElement content = null) {
            var obj = TryGetAssignable(key, content);
            if (obj == null) throw new Exception($"Invalid Lua assignable AST element '{key}'");
            return obj;
        }

        public static IXMLLuaSearchStatement TryGetStatement(string key, XmlElement content = null) {
            Type type;
            Statements.TryGetValue(key, out type);
            if (type == null) return null;
            var inst = Activator.CreateInstance(type) as IXMLLuaSearchStatement;
            if (content != null) inst.FillIn(content);
            return inst;
        }

        public static IXMLLuaSearchStatement GetStatement(string key, XmlElement content = null) {
            var obj = TryGetStatement(key, content);
            if (obj == null) throw new Exception($"Invalid Lua statement AST element '{key}'");
            return obj;
        }

        public static IXMLLuaSearchExpression TryGetExpression(string key, XmlElement content = null) {
            Type type;
            Expressions.TryGetValue(key, out type);
            if (type == null) return null;
            var inst = Activator.CreateInstance(type) as IXMLLuaSearchExpression;
            if (content != null) inst.FillIn(content);
            return inst;
        }

        public static IXMLLuaSearchExpression GetExpression(string key, XmlElement content = null) {
            var obj = TryGetExpression(key, content);
            if (obj == null) throw new Exception($"Invalid Lua expression AST element '{key}'");
            return obj;
        }

        public static XMLLuaSearchElement TryGet(string key) {
            var st = TryGetStatement(key);
            if (st != null) return st as XMLLuaSearchElement;
            return TryGetExpression(key) as XMLLuaSearchElement;
        }
    }
}
