using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class InvocationExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Method;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is not InvocationExpression invocationExpression || !typeof(Delegate).IsAssignableFrom(invocationExpression.Expression.Type))
                return null;

            return new MethodCallExpressionNode(context.Convert(invocationExpression.Expression), nameof(Action.Invoke),
                context.Convert(ItemOrIReadOnlyList.FromList(invocationExpression.Arguments)));
        }
    }
}