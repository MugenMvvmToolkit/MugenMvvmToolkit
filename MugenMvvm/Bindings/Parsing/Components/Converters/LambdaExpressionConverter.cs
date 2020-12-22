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
    public sealed class LambdaExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Lambda;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is LambdaExpression lambda))
                return null;
            var expressions = lambda.Parameters;
            try
            {
                var parameters = new IParameterExpressionNode[expressions.Count];
                for (var i = 0; i < expressions.Count; i++)
                {
                    var parameterExpression = expressions[i];
                    var parameterExpressionNode = new ParameterExpressionNode(parameterExpression.Name ?? "", null);
                    parameters[i] = parameterExpressionNode;
                    context.SetExpression(parameterExpression, parameterExpressionNode);
                }

                return new LambdaExpressionNode(context.Convert(lambda.Body), parameters, null);
            }
            finally
            {
                for (var i = 0; i < expressions.Count; i++)
                    context.ClearExpression(expressions[i]);
            }
        }

        #endregion
    }
}