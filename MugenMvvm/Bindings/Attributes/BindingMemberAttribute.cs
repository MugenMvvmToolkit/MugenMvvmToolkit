using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BindingMemberAttribute : BindingSyntaxExtensionAttributeBase
    {
        private IExpressionNode? _expressionNode;
        private bool _initialized;

        public BindingMemberAttribute(string memberName)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            MemberName = memberName;
            MemberNameIndex = -1;
        }

        public BindingMemberAttribute(int memberNameIndex)
        {
            Should.BeValid(memberNameIndex >= 0, nameof(memberNameIndex));
            MemberNameIndex = memberNameIndex;
        }

        public string? MemberName { get; }

        public int MemberNameIndex { get; }

        internal static IExpressionNode? GetTarget(IExpressionConverterContext<Expression> context, Expression? expression)
        {
            if (expression is MethodCallExpression method)
            {
                //ext method only
                if (method.Method.IsStatic && method.Arguments.Count > 0 && method.Method.IsDefined(typeof(ExtensionAttribute), false))
                    return context.Convert(method.Arguments[0]);

                if (method.Method.IsStatic)
                    return context.ConvertTarget(method.Object, method.Method);
                return context.ConvertOptional(method.Object);
            }

            if (expression is MemberExpression member)
            {
                if (member.Member.IsStatic())
                    return context.ConvertTarget(member.Expression, member.Member);
                return context.ConvertOptional(member.Expression);
            }

            return null;
        }

        protected override bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            if (!_initialized)
            {
                string? memberName;
                //T Resource<T>(this IBindingBuilderContext ctx, string resource);
                if (MemberNameIndex >= 0 && expression is MethodCallExpression methodCall &&
                    methodCall.Arguments.Count > MemberNameIndex && methodCall.Arguments[MemberNameIndex] is ConstantExpression constant && constant.Value != null)
                    memberName = constant.Value.ToString();
                else
                    memberName = MemberName;
                if (memberName != null)
                    _expressionNode = MemberExpressionNode.Get(GetTarget(context, expression), memberName);
                _initialized = true;
            }

            result = _expressionNode;
            return _expressionNode != null;
        }
    }
}