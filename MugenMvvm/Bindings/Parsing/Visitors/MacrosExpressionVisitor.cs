using System;
using System.Collections.Generic;
using System.Text;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Visitors
{
    public sealed class MacrosExpressionVisitor : IExpressionVisitor
    {
        private static readonly Dictionary<string, object?> AccessorMetadata = new(3)
        {
            {BindingParameterNameConstant.SuppressMethodAccessors, BoxingExtensions.FalseObject}
        };

        private readonly StringBuilder _memberBuilder;

        public MacrosExpressionVisitor()
        {
            _memberBuilder = new StringBuilder();
            var target = ConstantExpressionNode.Get(typeof(BindingMugenExtensions), typeof(Type));
            var bindingImpl = new MethodCallExpressionNode(target, nameof(BindingMugenExtensions.GetBinding), default);
            var eventArgsImpl = new MethodCallExpressionNode(target, nameof(BindingMugenExtensions.GetEventArgs), default);
            Macros = new Dictionary<string, Func<IReadOnlyMetadataContext?, IExpressionNode>>(3)
            {
                {MacrosConstant.Binding, _ => bindingImpl},
                {MacrosConstant.EventArgs, _ => eventArgsImpl},
                {MacrosConstant.Action, _ => new MemberExpressionNode(null, FakeMemberProvider.FakeMemberPrefixSymbol + Default.NextCounter().ToString())}
            };
            MethodAliases = new Dictionary<string, IMethodCallExpressionNode>(3)
            {
                {nameof(string.Format), new MethodCallExpressionNode(ConstantExpressionNode.Get<string>(), nameof(string.Format), default)},
                {nameof(Equals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(Equals), default)},
                {nameof(ReferenceEquals), new MethodCallExpressionNode(ConstantExpressionNode.Get<object>(), nameof(ReferenceEquals), default)}
            };
            AccessorMethods = new Dictionary<string, string>(11)
            {
                {nameof(BindableMembers.GetErrors), nameof(BindableMembers.GetErrors)},
                {nameof(BindableMembers.GetError), nameof(BindableMembers.GetError)},
                {nameof(BindableMembers.HasErrors), nameof(BindableMembers.HasErrors)},
                {"Rel", nameof(BindableMembers.RelativeSource)},
                {"Relative", nameof(BindableMembers.RelativeSource)},
                {nameof(BindableMembers.RelativeSource), nameof(BindableMembers.RelativeSource)},
                {"El", nameof(BindableMembers.ElementSource)},
                {"Element", nameof(BindableMembers.ElementSource)},
                {nameof(BindableMembers.ElementSource), nameof(BindableMembers.ElementSource)}
            };
            MacrosTargets = new Dictionary<string, IExpressionNode>(11)
            {
                {nameof(BindableMembers.GetErrors), UnaryExpressionNode.ContextMacros},
                {nameof(BindableMembers.GetError), UnaryExpressionNode.ContextMacros},
                {nameof(BindableMembers.HasErrors), UnaryExpressionNode.ContextMacros},
                {"Rel", UnaryExpressionNode.TargetMacros},
                {nameof(BindableMembers.RelativeSource), UnaryExpressionNode.TargetMacros},
                {"El", UnaryExpressionNode.TargetMacros},
                {"Element", UnaryExpressionNode.TargetMacros},
                {nameof(BindableMembers.ElementSource), UnaryExpressionNode.TargetMacros}
            };
        }

        public Dictionary<string, Func<IReadOnlyMetadataContext?, IExpressionNode>> Macros { get; }

        public Dictionary<string, IMethodCallExpressionNode> MethodAliases { get; }

        public Dictionary<string, string> AccessorMethods { get; }

        public Dictionary<string, IExpressionNode> MacrosTargets { get; }

        public ExpressionTraversalType TraversalType => ExpressionTraversalType.Postorder;

        public IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IMethodCallExpressionNode method && AccessorMethods.TryGetValue(method.Method, out var methodName))
            {
                var arguments = method.Arguments;
                if (arguments.Count == 0)
                {
                    if (method.Method == methodName)
                        return method.UpdateMetadata(method.Metadata.Merge(AccessorMetadata));
                    return new MethodCallExpressionNode(method.Target, methodName, default, method.TypeArgs, method.Metadata.Merge(AccessorMetadata));
                }

                if (method.Method == methodName && arguments.IsAllConstants())
                    return method.UpdateMetadata(method.Metadata.Merge(AccessorMetadata));

                var args = ItemOrArray.Get<IExpressionNode>(arguments.Count);
                for (var i = 0; i < args.Count; i++)
                {
                    var expressionNode = arguments[i];
                    if (expressionNode is IConstantExpressionNode)
                    {
                        args.SetAt(i, expressionNode);
                        continue;
                    }

                    if (!expressionNode.TryBuildBindingMemberPath(_memberBuilder, n => n is IMemberExpressionNode, out _))
                        return expression;

                    args.SetAt(i, ConstantExpressionNode.Get(_memberBuilder.GetPath(), typeof(string)));
                }

                return new MethodCallExpressionNode(method.Target, methodName, args, method.TypeArgs, method.Metadata.Merge(AccessorMetadata));
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
    }
}