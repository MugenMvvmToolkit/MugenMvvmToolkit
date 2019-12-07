using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ConstantTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Constructors

        public ConstantTokenParserComponent()
        {
            LiteralToExpression = new Dictionary<string, IExpressionNode>(3, StringComparer.Ordinal)
            {
                {"null", ConstantExpressionNode.Null},
                {"true", ConstantExpressionNode.True},
                {"false", ConstantExpressionNode.False}
            };
        }

        #endregion

        #region Properties

        public Dictionary<string, IExpressionNode> LiteralToExpression { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var start = context.SkipWhitespacesPosition();

            if (!context.IsIdentifier(out var end, start))
                return null;

            if (!LiteralToExpression.TryGetValue(context.GetValue(start, end), out expression))
                return null;

            context.Position = end;
            return expression;
        }

        #endregion
    }
}