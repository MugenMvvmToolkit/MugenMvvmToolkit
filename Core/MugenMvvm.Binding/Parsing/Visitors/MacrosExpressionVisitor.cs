using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class MacrosExpressionVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly StringBuilder _memberBuilder;

        #endregion

        #region Constructors

        public MacrosExpressionVisitor()
        {
            _memberBuilder = new StringBuilder();
            var target = ConstantExpressionNode.Get(typeof(MugenBindingExtensions), typeof(Type));
            Macros = new Dictionary<string, IExpressionNode>
            {
                {MacrosConstant.Binding, new MethodCallExpressionNode(target, nameof(MugenBindingExtensions.GetBinding), Default.EmptyArray<IExpressionNode>())},
                {MacrosConstant.EventArgs, new MethodCallExpressionNode(target, nameof(MugenBindingExtensions.GetEventArgs), Default.EmptyArray<IExpressionNode>())}
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
                {"El", BindableMembers.Object.ElementSource},
                {"Element", BindableMembers.Object.ElementSource}
            };
            MacrosTargets = new Dictionary<string, IExpressionNode>
            {
                {nameof(INotifyDataErrorInfo.GetErrors), UnaryExpressionNode.ContextMacros},
                {"HasErrors", UnaryExpressionNode.ContextMacros},
                {"Rel", UnaryExpressionNode.TargetMacros},
                {BindableMembers.Object.RelativeSource, UnaryExpressionNode.TargetMacros},
                {"El", UnaryExpressionNode.TargetMacros},
                {"Element", UnaryExpressionNode.TargetMacros},
                {BindableMembers.Object.ElementSource, UnaryExpressionNode.TargetMacros}
            };
        }

        #endregion

        #region Properties

        bool IExpressionVisitor.IsPostOrder => true;

        public Dictionary<string, IExpressionNode> Macros { get; }

        public Dictionary<string, IMethodCallExpressionNode> MethodAliases { get; }

        public Dictionary<string, string> ConstantParametersMethods { get; }

        public Dictionary<string, IExpressionNode> MacrosTargets { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IMethodCallExpressionNode method && ConstantParametersMethods.TryGetValue(method.Method, out var methodName))
            {
                var arguments = method.Arguments;
                if (arguments.Count == 0)
                {
                    if (method.Method == methodName)
                        return method;
                    return new MethodCallExpressionNode(method.Target, methodName, Default.EmptyArray<IExpressionNode>(), method.TypeArgs);
                }

                if (method.Method == methodName && arguments.All(n => n is IConstantExpressionNode))
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

                    if (!expressionNode.TryBuildBindingMemberPath(_memberBuilder, n => n is IMemberExpressionNode, out _))
                        return expression;

                    args[i] = ConstantExpressionNode.Get(_memberBuilder.GetPath(), typeof(string));
                }

                return new MethodCallExpressionNode(method.Target, methodName, args, method.TypeArgs);
            }

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
            {
                if (unaryExpression.Operand is IMemberExpressionNode memberExpression && Macros.TryGetValue(memberExpression.Member, out var member))
                    return member;

                if (unaryExpression.Operand is IMethodCallExpressionNode methodCallExpression)
                {
                    if (MethodAliases.TryGetValue(methodCallExpression.Method, out var m))
                        return m.UpdateArguments(methodCallExpression.Arguments);

                    if (methodCallExpression.Target == null && MacrosTargets.TryGetValue(methodCallExpression.Method, out var target))
                        return methodCallExpression.UpdateTarget(target);
                }
            }

            return expression;
        }

        #endregion
    }
}