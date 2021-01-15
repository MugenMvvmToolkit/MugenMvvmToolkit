using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class MethodCallExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; set; } = ParsingComponentPriority.Method;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is not MethodCallExpression methodCallExpression)
                return null;

            if (context.TryConvertExtension(methodCallExpression.Method, expression, out var result))
                return result;

            return context.ConvertMethodCall(methodCallExpression);
        }
    }
}