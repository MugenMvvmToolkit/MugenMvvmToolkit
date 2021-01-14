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
    public sealed class DefaultExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; set; } = ParsingComponentPriority.Convert;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is DefaultExpression d)
                return ConstantExpressionNode.Get(d.Type.GetDefaultValue(), d.Type);
            return null;
        }
    }
}