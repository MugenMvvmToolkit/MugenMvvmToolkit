using System.Linq.Expressions;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Converters
{
    public sealed class IndexerExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Indexer;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is IndexExpression index))
                return null;

            var attribute = BindingSyntaxExtensionAttributeBase.TryGet(index.Indexer);
            if (attribute != null && attribute.TryConvert(context, expression, out var result))
                return result;

            return new IndexExpressionNode(context.ConvertTarget(index.Object, index.Indexer), context.Convert(index.Arguments));
        }

        #endregion
    }
}