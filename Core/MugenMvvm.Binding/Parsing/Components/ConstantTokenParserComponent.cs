using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ConstantTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Constructors

        public ConstantTokenParserComponent(Dictionary<string, IExpressionNode>? literalToExpression = null)
        {
            if (literalToExpression == null)
            {
                LiteralToExpression = new Dictionary<string, IExpressionNode>
                {
                    {"null", new ConstantExpressionNode(null, typeof(object))},
                    {bool.TrueString, new ConstantExpressionNode(Default.TrueObject, typeof(bool))},
                    {bool.FalseString, new ConstantExpressionNode(Default.FalseObject, typeof(bool))}
                };
            }
            else
                LiteralToExpression = literalToExpression;
        }

        #endregion

        #region Properties

        public Dictionary<string, IExpressionNode> LiteralToExpression { get; }

        public int Priority { get; set; } = BindingParserPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var start = context.SkipWhitespacesPosition();

            if (!context.IsIdentifier(out var end, start))
                return null;

            if (!LiteralToExpression.TryGetValue(context.GetValue(start, end), out expression))
                return null;

            context.SetPosition(end);
            return expression;
        }

        #endregion
    }
}