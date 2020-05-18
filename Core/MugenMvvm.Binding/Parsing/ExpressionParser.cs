using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Extensions.Components;
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

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IExpressionParserComponent>(metadata).TryParse(expression, metadata);
        }

        #endregion
    }
}