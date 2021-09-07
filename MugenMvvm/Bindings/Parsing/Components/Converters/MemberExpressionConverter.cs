using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class MemberExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Member;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is not MemberExpression memberExpression)
                return null;

            if (memberExpression.Member.Name == nameof(Nullable<bool>.Value) && memberExpression.Member.DeclaringType != null &&
                memberExpression.Member.DeclaringType.IsNullableType())
                return context.ConvertTarget(memberExpression.Expression, memberExpression.Member);

            if (context.TryConvertExtension(memberExpression.Member, expression, out var result))
                return result;

            return MemberExpressionNode.Get(context.ConvertTarget(memberExpression.Expression, memberExpression.Member), memberExpression.Member.Name,
                memberExpression.Member.GetMemberFlagsMetadata(false));
        }
    }
}