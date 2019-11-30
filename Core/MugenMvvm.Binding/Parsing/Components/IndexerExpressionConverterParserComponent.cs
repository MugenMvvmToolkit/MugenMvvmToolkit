using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class IndexerExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Indexer;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is IndexExpression index)
                return new IndexExpressionNode(context.ConvertTarget(index.Object, index.Indexer), context.Convert(index.Arguments));
            return null;
        }

        #endregion
    }
}