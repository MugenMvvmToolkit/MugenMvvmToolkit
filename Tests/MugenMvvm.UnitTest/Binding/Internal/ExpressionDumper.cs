using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.UnitTest.Binding.Internal
{
    public static class ExpressionDumper
    {
        #region Methods

        public static string ToCode(IExpressionNode? expression, bool valueToConstant)
        {
            if (expression == null)
                return "null";
            switch (expression)
            {
                case IBinaryExpressionNode ex:
                    return $"new BinaryExpressionNode({ToCode(ex.Token)}, {ToCode(ex.Left, valueToConstant)}, {ToCode(ex.Right, valueToConstant)})";
                case IConditionExpressionNode ex:
                    return $"new ConditionExpressionNode({ToCode(ex.Condition, valueToConstant)}, {ToCode(ex.IfTrue, valueToConstant)}, {ToCode(ex.IfFalse, valueToConstant)})";
                case IConstantExpressionNode ex:
                    return $"ConstantExpressionNode.Get({ToCodeValue(ex.Value)}, typeof({ex.Type.Name}))";
                case IIndexExpressionNode ex:
                    return $"new IndexExpressionNode({ToCode(ex.Target, valueToConstant)}, {ToCode(ex.Arguments, valueToConstant)})";
                case ILambdaExpressionNode ex:
                    return $"new LambdaExpressionNode({ToCode(ex.Body, valueToConstant)}, {ToCode(ex.Parameters, valueToConstant)})";
                case IMemberExpressionNode ex:
                    if (valueToConstant && ex.Member.StartsWith("value"))
                        return $"ConstantExpressionNode.Get({ex.Member})";
                    return $"new MemberExpressionNode({ToCode(ex.Target, valueToConstant)}, \"{ex.Member}\")";
                case IMethodCallExpressionNode ex:
                    return $"new MethodCallExpressionNode({ToCode(ex.Target, valueToConstant)}, \"{ex.Method}\", {ToCode(ex.Arguments, valueToConstant)}, {ToCode(ex.TypeArgs)})";
                case IUnaryExpressionNode ex:
                    return $"new UnaryExpressionNode({ToCode(ex.Token)}, {ToCode(ex.Operand, valueToConstant)})";
                case IParameterExpressionNode ex:
                    return $"new ParameterExpressionNode(\"{ex.Name}\")";
                default:
                    throw new NotSupportedException();
            }
        }

        private static string ToCodeValue(object? value)
        {
            if (value == null)
                return "null";
            if (value is string)
                return "\"" + value + "\"";
            if (value is Type type)
                return $"typeof({type.Name})";
            if (value is bool)
                return value.ToString().ToLowerInvariant();
            return value.ToString();
        }

        private static string ToCode(BinaryTokenType token)
        {
            var fieldInfo = typeof(BinaryTokenType).GetFields().First(info => Equals(info.GetValue(null), token));
            return $"BinaryTokenType.{fieldInfo.Name}";
        }

        private static string ToCode(UnaryTokenType token)
        {
            var fieldInfo = typeof(UnaryTokenType).GetFields().First(info => Equals(info.GetValue(null), token));
            return $"UnaryTokenType.{fieldInfo.Name}";
        }

        private static string ToCode(IEnumerable<string> expressions)
        {
            if (!expressions.Any())
                return "new string[0]";
            var stringBuilder = new StringBuilder("new string[] {");
            foreach (var expressionNode in expressions)
                stringBuilder.Append("\"").Append(expressionNode).Append("\"").Append(",");
            stringBuilder.Remove(stringBuilder.Length - 1, 1).Append("}");
            return stringBuilder.ToString();
        }

        private static string ToCode<T>(IEnumerable<T> expressions, bool valueToConstant) where T : class, IExpressionNode
        {
            if (!expressions.Any())
                return $"new {typeof(T).Name}[0]";
            var stringBuilder = new StringBuilder($"new {typeof(T).Name}[] {{");
            foreach (var expressionNode in expressions)
                stringBuilder.Append(ToCode(expressionNode, valueToConstant)).Append(",");
            stringBuilder.Remove(stringBuilder.Length - 1, 1).Append("}");
            return stringBuilder.ToString();
        }

        #endregion
    }
}