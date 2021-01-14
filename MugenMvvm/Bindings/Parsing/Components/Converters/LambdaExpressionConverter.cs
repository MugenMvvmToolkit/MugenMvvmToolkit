using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class LambdaExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public int Priority { get; set; } = ParsingComponentPriority.Lambda;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is LambdaExpression lambda))
                return null;
            var expressions = lambda.Parameters;
            try
            {
                var parameters = ItemOrArray.Get<IParameterExpressionNode>(expressions.Count);
                for (var i = 0; i < expressions.Count; i++)
                {
                    var parameterExpression = expressions[i];
                    var parameterExpressionNode = new ParameterExpressionNode(parameterExpression.Name ?? "");
                    parameters.SetAt(i, parameterExpressionNode);
                    context.SetExpression(parameterExpression, parameterExpressionNode);
                }

                return new LambdaExpressionNode(context.Convert(lambda.Body), parameters);
            }
            finally
            {
                for (var i = 0; i < expressions.Count; i++)
                    context.ClearExpression(expressions[i]);
            }
        }
    }
}