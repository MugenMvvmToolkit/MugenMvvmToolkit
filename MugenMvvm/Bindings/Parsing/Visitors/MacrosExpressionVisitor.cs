using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Visitors
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
            var target = ConstantExpressionNode.Get(typeof(BindingMugenExtensions), typeof(Type));
            var bindingImpl = new MethodCallExpressionNode(target, nameof(BindingMugenExtensions.GetBinding), Default.Array<IExpressionNode>(), null, Default.ReadOnlyDictionary<string, object?>());
            var eventArgsImpl = new MethodCallExpressionNode(target, nameof(BindingMugenExtensions.GetEventArgs), Default.Array<IExpressionNode>(), null, Default.ReadOnlyDictionary<string, object?>());
            Macros = new Dictionary<string, Func<IReadOnlyMetadataContext?, IExpressionNode>>
            {
                {MacrosConstant.Binding, context => bindingImpl},
                {MacrosConstant.EventArgs, context => eventArgsImpl},
                {MacrosConstant.Action, context => new MemberExpressionNode(null, FakeMemberProvider.FakeMemberPrefixSymbol + Default.NextCounter().ToString())}
            };
            MethodAliases = new Dictionary<string, IMethodCallExpressionNode>
            {
                {nameof(string.Format), new MethodCallExpressionNode(ConstantExpressionNode.Get<string>(), nameof(string.Format), Default.Array<IExpressionNode>())},
                {nameof(Equals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(Equals), Default.Array<IExpressionNode>())},
                {nameof(ReferenceEquals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(ReferenceEquals), Default.Array<IExpressionNode>())}
            };
            ConstantParametersMethods = new Dictionary<string, string>
            {
                {BindableMembers.For<object>().GetErrorsMethod(), BindableMembers.For<object>().GetErrorsMethod()},
                {BindableMembers.For<object>().GetErrorMethod(), BindableMembers.For<object>().GetErrorMethod()},
                {BindableMembers.For<object>().HasErrorsMethod(), BindableMembers.For<object>().HasErrorsMethod()},
                {"Rel", BindableMembers.For<object>().RelativeSourceMethod()},
                {"Relative", BindableMembers.For<object>().RelativeSourceMethod()},
                {"El", BindableMembers.For<object>().ElementSourceMethod()},
                {"Element", BindableMembers.For<object>().ElementSourceMethod()}
            };
            MacrosTargets = new Dictionary<string, IExpressionNode>
            {
                {BindableMembers.For<object>().GetErrorsMethod(), UnaryExpressionNode.ContextMacros},
                {BindableMembers.For<object>().GetErrorMethod(), UnaryExpressionNode.ContextMacros},
                {BindableMembers.For<object>().HasErrorsMethod(), UnaryExpressionNode.ContextMacros},
                {"Rel", UnaryExpressionNode.TargetMacros},
                {BindableMembers.For<object>().RelativeSourceMethod(), UnaryExpressionNode.TargetMacros},
                {"El", UnaryExpressionNode.TargetMacros},
                {"Element", UnaryExpressionNode.TargetMacros},
                {BindableMembers.For<object>().ElementSourceMethod(), UnaryExpressionNode.TargetMacros}
            };
        }

        #endregion

        #region Properties

        bool IExpressionVisitor.IsPostOrder => true;

        public Dictionary<string, Func<IReadOnlyMetadataContext?, IExpressionNode>> Macros { get; }

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
                    return new MethodCallExpressionNode(method.Target, methodName, Default.Array<IExpressionNode>(), method.TypeArgs, method.Metadata);
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

                return new MethodCallExpressionNode(method.Target, methodName, args, method.TypeArgs, method.Metadata);
            }

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
            {
                if (unaryExpression.Operand is IMemberExpressionNode memberExpression && Macros.TryGetValue(memberExpression.Member, out var getMacros))
                    return getMacros(metadata);

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