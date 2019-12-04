using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class ExpressionParser : ComponentOwnerBase<IExpressionParser>, IExpressionParser
    {
        #region Constructors

        public ExpressionParser(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            var parsers = GetComponents<IExpressionParserComponent>(metadata);
            for (var i = 0; i < parsers.Length; i++)
            {
                var result = parsers[i].TryParse(expression, metadata);
                if (!result.Item.IsEmpty || result.List != null)
                    return result;
            }

            BindingExceptionManager.ThrowCannotParseExpression(expression);
            return default;
        }

        #endregion
    }
}