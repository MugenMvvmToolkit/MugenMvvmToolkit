using System.Collections.Generic;
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

        public ExpressionParser(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(object expression, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IExpressionParserComponent>(metadata).TryParse(this, expression, metadata);

        #endregion
    }
}