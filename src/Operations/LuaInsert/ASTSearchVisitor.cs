using System;
using NetLua.Ast;
using SourceSpan = Irony.Parsing.SourceSpan;
using ModTheGungeon;

namespace Ommel {
	public class ASTSearchWalker {
		public XMLLuaSearchElement Element;
		public static Logger Logger = new Logger("ASTSearchWalker");
		public SourceSpan? SelectedSpan;
		public XMLLuaSearchElement SelectedElement;

		static ASTSearchWalker() {
			Logger.MaxLogLevel = Logger.LogLevel.Warn;
		}

		public bool Match(XMLLuaSearchElement req, object real) {
			Logger.Debug($"Match element {req} -> object {real}");
			if (req is XMLLuaSearchFunctionCall && real is FunctionCall) return Match((XMLLuaSearchFunctionCall)req, (FunctionCall)real);
			if (req is XMLLuaSearchAssignment && real is Assignment) return Match((XMLLuaSearchAssignment)req, (Assignment)real);
			if (req is XMLLuaSearchReturnStatement && real is ReturnStat) return Match((XMLLuaSearchReturnStatement)req, (ReturnStat)real);
			if (req is XMLLuaSearchBreakStatement && real is BreakStat) return Match((XMLLuaSearchBreakStatement)req, (BreakStat)real);
			if (req is XMLLuaSearchLocalAssignment && real is LocalAssignment) return Match((XMLLuaSearchLocalAssignment)req, (LocalAssignment)real);
			if (req is XMLLuaSearchBlock && real is Block) return Match((XMLLuaSearchBlock)req, (Block)real);
			if (req is XMLLuaSearchWhileStatement && real is WhileStat) return Match((XMLLuaSearchWhileStatement)req, (WhileStat)real);
			if (req is XMLLuaSearchRepeatStatement && real is RepeatStat) return Match((XMLLuaSearchRepeatStatement)req, (RepeatStat)real);
			if (req is XMLLuaSearchNumericFor && real is NumericFor) return Match((XMLLuaSearchNumericFor)req, (NumericFor)real);
			if (req is XMLLuaSearchGenericFor && real is GenericFor) return Match((XMLLuaSearchGenericFor)req, (GenericFor)real);
			if (req is XMLLuaSearchIfStatement && real is IfStat) return Match((XMLLuaSearchIfStatement)req, (IfStat)real);
			if (req is XMLLuaSearchVariable && real is Variable) return Match((XMLLuaSearchVariable)req, (Variable)real);
			if (req is XMLLuaSearchStringLiteral && real is StringLiteral) return Match((XMLLuaSearchStringLiteral)req, (StringLiteral)real);
			if (req is XMLLuaSearchNumberLiteral && real is NumberLiteral) return Match((XMLLuaSearchNumberLiteral)req, (NumberLiteral)real);
			if (req is XMLLuaSearchNilLiteral && real is NilLiteral) return Match((XMLLuaSearchNilLiteral)req, (NilLiteral)real);
			if (req is XMLLuaSearchBoolLiteral && real is BoolLiteral) return Match((XMLLuaSearchBoolLiteral)req, (BoolLiteral)real);
			if (req is XMLLuaSearchVarargsLiteral && real is VarargsLiteral) return Match((XMLLuaSearchVarargsLiteral)req, (VarargsLiteral)real);
			if (req is XMLLuaSearchTableAccess && real is TableAccess) return Match((XMLLuaSearchTableAccess)req, (TableAccess)real);
			if (req is XMLLuaSearchFunctionDefinition && real is FunctionDefinition) return Match((XMLLuaSearchFunctionDefinition)req, (FunctionDefinition)real);
			if (req is XMLLuaSearchBinaryExpression && real is BinaryExpression) return Match((XMLLuaSearchBinaryExpression)req, (BinaryExpression)real);
			if (req is XMLLuaSearchUnaryExpression && real is UnaryExpression) return Match((XMLLuaSearchUnaryExpression)req, (UnaryExpression)real);
			if (req is XMLLuaSearchTableConstructor && real is TableConstructor) return Match((XMLLuaSearchTableConstructor)req, (TableConstructor)real);
			return false;
		}

		public bool Match(IXMLLuaSearchStatement req, IStatement real) {
			Logger.Debug($"Match statement {req} -> {real}");
			if (req is XMLLuaSearchFunctionCall && real is FunctionCall) return Match((XMLLuaSearchFunctionCall)req, (FunctionCall)real);
			if (req is XMLLuaSearchAssignment && real is Assignment) return Match((XMLLuaSearchAssignment)req, (Assignment)real);
			if (req is XMLLuaSearchReturnStatement && real is ReturnStat) return Match((XMLLuaSearchReturnStatement)req, (ReturnStat)real);
			if (req is XMLLuaSearchBreakStatement && real is BreakStat) return Match((XMLLuaSearchBreakStatement)req, (BreakStat)real);
			if (req is XMLLuaSearchLocalAssignment && real is LocalAssignment) return Match((XMLLuaSearchLocalAssignment)req, (LocalAssignment)real);
			if (req is XMLLuaSearchBlock && real is Block) return Match((XMLLuaSearchBlock)req, (Block)real);
			if (req is XMLLuaSearchWhileStatement && real is WhileStat) return Match((XMLLuaSearchWhileStatement)req, (WhileStat)real);
			if (req is XMLLuaSearchRepeatStatement && real is RepeatStat) return Match((XMLLuaSearchRepeatStatement)req, (RepeatStat)real);
			if (req is XMLLuaSearchNumericFor && real is NumericFor) return Match((XMLLuaSearchNumericFor)req, (NumericFor)real);
			if (req is XMLLuaSearchGenericFor && real is GenericFor) return Match((XMLLuaSearchGenericFor)req, (GenericFor)real);
			if (req is XMLLuaSearchIfStatement && real is IfStat) return Match((XMLLuaSearchIfStatement)req, (IfStat)real);
			return false;
		}

		public bool Match(IXMLLuaSearchExpression req, IExpression real) {
			Logger.Debug($"Match expression {req} -> {real}");
			if (req is XMLLuaSearchVariable && real is Variable) return Match((XMLLuaSearchVariable)req, (Variable)real);
			if (req is XMLLuaSearchStringLiteral && real is StringLiteral) return Match((XMLLuaSearchStringLiteral)req, (StringLiteral)real);
			if (req is XMLLuaSearchNumberLiteral && real is NumberLiteral) return Match((XMLLuaSearchNumberLiteral)req, (NumberLiteral)real);
			if (req is XMLLuaSearchNilLiteral && real is NilLiteral) return Match((XMLLuaSearchNilLiteral)req, (NilLiteral)real);
			if (req is XMLLuaSearchBoolLiteral && real is BoolLiteral) return Match((XMLLuaSearchBoolLiteral)req, (BoolLiteral)real);
			if (req is XMLLuaSearchVarargsLiteral && real is VarargsLiteral) return Match((XMLLuaSearchVarargsLiteral)req, (VarargsLiteral)real);
			if (req is XMLLuaSearchFunctionCall && real is FunctionCall) return Match((XMLLuaSearchFunctionCall)req, (FunctionCall)real);
			if (req is XMLLuaSearchTableAccess && real is TableAccess) return Match((XMLLuaSearchTableAccess)req, (TableAccess)real);
			if (req is XMLLuaSearchFunctionDefinition && real is FunctionDefinition) return Match((XMLLuaSearchFunctionDefinition)req, (FunctionDefinition)real);
			if (req is XMLLuaSearchBinaryExpression && real is BinaryExpression) return Match((XMLLuaSearchBinaryExpression)req, (BinaryExpression)real);
			if (req is XMLLuaSearchUnaryExpression && real is UnaryExpression) return Match((XMLLuaSearchUnaryExpression)req, (UnaryExpression)real);
			if (req is XMLLuaSearchTableConstructor && real is TableConstructor) return Match((XMLLuaSearchTableConstructor)req, (TableConstructor)real);
			return false;
		}

		public bool Match(IXMLLuaSearchAssignable req, IAssignable real) {
			Logger.Debug($"Match assignable {req} -> {real}");
			if (req is XMLLuaSearchVariable && real is Variable) return Match((XMLLuaSearchVariable)req, (Variable)real);
			if (req is XMLLuaSearchTableAccess && real is TableAccess) return Match((XMLLuaSearchTableAccess)req, (TableAccess)real);
			return false;
		}

		public bool Match(XMLLuaSearchNumberLiteral req, NumberLiteral real) {
			Logger.Debug($"number_literal");
			if (req.Value == null) return false;
			if (Math.Abs(req.Value.Value - real.Value) < Double.Epsilon) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchBoolLiteral req, BoolLiteral real) {
			Logger.Debug($"bool_literal");
			if (req.Value == real.Value) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchFunctionCall req, FunctionCall real) {
			Logger.Debug($"function_call");
			if (req.Arguments != null) {
				if (req.Arguments.Count != real.Arguments.Count) return false;
				for (var i = 0; i < req.Arguments.Count; i++) {
					if (!Match(req.Arguments[i], real.Arguments[i])) return false;
				}
			}

			if (req.Function == null || Match(req.Function, real.Function)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchFunctionDefinition req, FunctionDefinition real) {
			Logger.Debug($"function_definition");
			if (req.Arguments.Count != real.Arguments.Count) return false;
			for (var i = 0; i < req.Arguments.Count; i++) {
				if (req.Arguments[i] != real.Arguments[i].Name) return false;
			}
			if (Match(req.Body, real.Body)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchUnaryExpression req, UnaryExpression real) {
			Logger.Debug($"unary_expression");
			if (!Match(req.Expression, real.Expression)) return false;
			if (req.Operation == real.Operation) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchAssignment req, Assignment real) {
			Logger.Debug($"assignment");
			if (req.Expressions != null && (req.Expressions.Count != real.Expressions.Count)) return false;
			if (req.Variables != null && (req.Variables.Count != real.Variables.Count)) return false;
			if (req.Expressions != null) {
				for (var i = 0; i < req.Expressions.Count; i++) {
					if (!Match(req.Expressions[i], real.Expressions[i])) return false;
				}
			}
			if (req.Variables != null) {
				for (var i = 0; i < req.Variables.Count; i++) {
					if (!Match(req.Variables[i], real.Variables[i])) return false;
				}
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchBreakStatement req, BreakStat real) {
			Logger.Debug($"break_statement");
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchBlock req, Block real) {
			Logger.Debug($"block");
			if (req.Statements == null) return false;
			if (req.Full && (req.Statements.Count != real.Statements.Count)) return false;
			if (!req.Full && req.Statements.Count == 0) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}

			var j = 0;

			for (var i = 0; i < real.Statements.Count; i++) {
				var stat = real.Statements[i];

				if (Match(req.Statements[j], stat)) j += 1;
				else {
					j = 0;
					if (req.Full) return false;
				}

				if (j >= req.Statements.Count) {
					SetSelectionIfSelected(real.Span, req);
					return true;
				}
			}

			return false;
		}

		public bool Match(XMLLuaSearchRepeatStatement req, RepeatStat real) {
			Logger.Debug($"repeat_statement");
			if (Match(req.Block, real.Block) && Match(req.Condition, real.Condition)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchGenericFor req, GenericFor real) {
			Logger.Debug($"generic_for");
			if (!Match(req.Block, real.Block)) return false;
			if (req.Expressions.Count != real.Expressions.Count) return false;
			if (req.Variables.Count != real.Variables.Count) return false;
			for (var i = 0; i < req.Expressions.Count; i++) {
				if (!Match(req.Expressions[i], real.Expressions[i])) return false;
			}
			for (var i = 0; i < req.Variables.Count; i++) {
				if (req.Variables[i] != real.Variables[i]) return false;
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchIfStatement req, IfStat real) {
			Logger.Debug($"if_statement");
			if (!Match(req.Block, real.Block)) return false;
			if (!Match(req.Condition, real.Condition)) return false;
			if (!Match(req.Else, real.ElseBlock)) return false;
			if (req.ElseIfs.Count != real.ElseIfs.Count) return false;

			for (var i = 0; i < req.ElseIfs.Count; i++) {
				if (!Match(req.ElseIfs[i], real.ElseIfs[i])) return false;
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchNumericFor req, NumericFor real) {
			Logger.Debug($"numeric_for");
			if (Match(req.Block, real.Block) && Match(req.Limit, real.Limit) && Match(req.Step, real.Step)
				&& Match(req.Var, real.Var) && (req.Variable == real.Variable)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchWhileStatement req, WhileStat real) {
			Logger.Debug($"while_statement");
			if (Match(req.Block, real.Block) && Match(req.Condition, real.Condition)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchLocalAssignment req, LocalAssignment real) {
			Logger.Debug($"local_assignment");
			if (req.Expressions.Count != real.Values.Count) return false;
			if (req.Names.Count != real.Names.Count) return false;
			for (var i = 0; i < req.Expressions.Count; i++) {
				if (!Match(req.Expressions[i], real.Values[i])) return false;
			}
			for (var i = 0; i < req.Names.Count; i++) {
				if (req.Names[i] != real.Names[i]) return false;
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchReturnStatement req, ReturnStat real) {
			Logger.Debug($"return_statement");
			if (req.Expressions.Count != real.Expressions.Count) return false;
			for (var i = 0; i < req.Expressions.Count; i++) {
				if (!Match(req.Expressions[i], real.Expressions[i])) return false;
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchTableConstructor req, TableConstructor real) {
			Logger.Debug($"table_constructor");
			if (req.Values.Count != real.Values.Count) return false;
			for (var i = 0; i < req.Values.Count; i++) {
				var value = req.Values[i];
				var found = false;

				foreach (var kv in real.Values) {
					if ((value.Key == null || Match(value.Key, kv.Key)) && (value.Value == null || Match(value.Value, kv.Value))) {
						found = true;
						break;
					}
				}
				if (!found) return false;
			}
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchBinaryExpression req, BinaryExpression real) {
			Logger.Debug($"left_binary_expression");
			if (Match(req.Left, real.Left) && Match(req.Right, real.Right) && req.Operation == real.Operation) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchTableAccess req, TableAccess real) {
			Logger.Debug($"table_access");
			if (Match(req.Index, real.Index) && Match(req.Table, real.Expression)) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchVarargsLiteral req, VarargsLiteral real) {
			Logger.Debug($"varargs_literal");
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchNilLiteral req, NilLiteral real) {
			Logger.Debug($"nil_literal");
			SetSelectionIfSelected(real.Span, req);
			return true;
		}

		public bool Match(XMLLuaSearchStringLiteral req, StringLiteral real) {
			Logger.Debug($"string_literal");
			if (req.Value == real.Value) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		public bool Match(XMLLuaSearchVariable req, Variable real) {
			Logger.Debug($"variable");
			if ((req.Name == null || req.Name == real.Name) && (req.Prefix == null || Match(req.Prefix, real.Prefix))) {
				SetSelectionIfSelected(real.Span, req);
				return true;
			}
			return false;
		}

		private void SetSelectionIfSelected(SourceSpan span, XMLLuaSearchElement elem) {
			if (elem.Select) {
				if (SelectedSpan != null) throw new Exception("Cannot select more than one AST element!");

				SelectedSpan = span;
				SelectedElement = elem;
			}

		}

		public void ResetSelection() {
			SelectedSpan = null;
			SelectedElement = null;
		}

		public bool HasSelection {
			get { return SelectedSpan != null; }
		}
	}
}
