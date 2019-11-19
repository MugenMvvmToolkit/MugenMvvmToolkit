using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public class MacrosExpressionVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly StringBuilder _memberBuilder;
        protected static readonly UnaryExpressionNode TargetMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Self);
        protected static readonly UnaryExpressionNode ContextMacros = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, MemberExpressionNode.Context);

        #endregion

        #region Constructors

        public MacrosExpressionVisitor()
        {
            _memberBuilder = new StringBuilder();
            var argsMethod = new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(MugenBindingExtensions), typeof(Type)), nameof(MugenBindingExtensions.GetEventArgs),
                Default.EmptyArray<IExpressionNode>());
            MacrosMethods = new Dictionary<string, IMethodCallExpressionNode>
            {
                {MacrosConstants.Binding, new MethodCallExpressionNode(argsMethod.Target, nameof(MugenBindingExtensions.GetBinding), Default.EmptyArray<IExpressionNode>())},
                {MacrosConstants.Args, argsMethod},
                {MacrosConstants.Arg, argsMethod}
            };
            MethodAliases = new Dictionary<string, IMethodCallExpressionNode>
            {
                {nameof(string.Format), new MethodCallExpressionNode(ConstantExpressionNode.Get<string>(), nameof(string.Format), Default.EmptyArray<IExpressionNode>())},
                {nameof(Equals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(Equals), Default.EmptyArray<IExpressionNode>())},
                {nameof(ReferenceEquals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(ReferenceEquals), Default.EmptyArray<IExpressionNode>())}
            };
            ConstantParametersMethods = new Dictionary<string, string>
            {
                {nameof(INotifyDataErrorInfo.GetErrors), nameof(INotifyDataErrorInfo.GetErrors)},
                {"HasErrors", "HasErrors"},
                {"Rel", BindableMembers.Object.RelativeSource},
                {"Relative", BindableMembers.Object.RelativeSource},
                {"RelativeSourceType", BindableMembers.Object.RelativeSource},
                {"El", BindableMembers.Object.ElementSource},
                {"Element", BindableMembers.Object.ElementSource},
                {"ElementSourceType", BindableMembers.Object.ElementSource}
            };
            MacrosTargets = new Dictionary<string, IExpressionNode>
            {
                {nameof(INotifyDataErrorInfo.GetErrors), ContextMacros},
                {"HasErrors", ContextMacros},
                {"Rel", TargetMacros},
                {"Relative", TargetMacros},
                {"RelativeSource", TargetMacros},
                {"El", TargetMacros},
                {"Element", TargetMacros},
                {"ElementSource", TargetMacros}
            };
        }

        #endregion

        #region Properties

        public bool IsPostOrder => true;

        public Dictionary<string, IMethodCallExpressionNode> MacrosMethods { get; }

        public Dictionary<string, IMethodCallExpressionNode> MethodAliases { get; }

        public Dictionary<string, string> ConstantParametersMethods { get; }

        public Dictionary<string, IExpressionNode> MacrosTargets { get; }

        #endregion

        #region Implementation of interfaces

        public virtual IExpressionNode? Visit(IExpressionNode expression)
        {
            if (expression is IMethodCallExpressionNode method && ConstantParametersMethods.TryGetValue(method.MethodName, out var methodName))
            {
                var arguments = method.Arguments;
                if (arguments.Count == 0)
                {
                    if (method.MethodName == methodName)
                        return method;
                    return new MethodCallExpressionNode(method.Target, methodName, Default.EmptyArray<IExpressionNode>(), method.TypeArgs);
                }

                if (method.MethodName == methodName && arguments.All(n => n is IConstantExpressionNode))
                    return method;

                var args = new IExpressionNode[arguments.Count];
                for (var i = 0; i < args.Length; i++)
                {
                    var expressionNode = arguments[i];
                    if (expressionNode is IConstantExpressionNode)
                    {
                        args[i] = expressionNode;
                        continue;
                    }

                    if (!expressionNode.TryBuildBindingMember(_memberBuilder, n => n is IMemberExpressionNode, out _))
                        return expression;

                    args[i] = ConstantExpressionNode.Get(_memberBuilder.GetPath(), typeof(string));
                }

                return new MethodCallExpressionNode(method.Target, methodName, args, method.TypeArgs);
            }

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
            {
                if (unaryExpression.Operand is IMemberExpressionNode memberExpression && MacrosMethods.TryGetValue(memberExpression.MemberName, out var m))
                    return m;

                if (unaryExpression.Operand is IMethodCallExpressionNode methodCallExpression)
                {
                    if (MethodAliases.TryGetValue(methodCallExpression.MethodName, out m))
                        return m.UpdateArguments(methodCallExpression.Arguments);

                    if (methodCallExpression.Target == null && MacrosTargets.TryGetValue(methodCallExpression.MethodName, out var target))
                        return methodCallExpression.UpdateTarget(target);
                }
            }

            return expression;
        }

        #endregion
    }
}