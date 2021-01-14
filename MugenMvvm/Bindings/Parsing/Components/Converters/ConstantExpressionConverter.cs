using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class ConstantExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is ConstantExpression c)
                return new ConstantExpressionNode(c.Value, c.Type, c);
            return null;
        }
    }
}