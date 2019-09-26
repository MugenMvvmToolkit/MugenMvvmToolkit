using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ConstantTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, IExpressionNode> _literalToExpression;

        #endregion

        #region Constructors

        public ConstantTokenParserComponent(Dictionary<string, IExpressionNode>? literalToExpression = null)
        {
            if (literalToExpression == null)
            {
                _literalToExpression = new Dictionary<string, IExpressionNode>
                {
                    {"null", new ConstantExpressionNode(null, typeof(object))},
                    {bool.TrueString, new ConstantExpressionNode(Default.TrueObject, typeof(bool))},
                    {bool.FalseString, new ConstantExpressionNode(Default.FalseObject, typeof(bool))}
                };
            }
            else
                _literalToExpression = literalToExpression;
        }

        #endregion

        #region Properties

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

            if (!_literalToExpression.TryGetValue(context.GetValue(start, end), out expression))
                return null;

            context.SetPosition(end);
            return expression;
        }

        #endregion
    }
}