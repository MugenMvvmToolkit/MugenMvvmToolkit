using System.Collections.Generic;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    internal interface IExpressionParserComponentInternal<TExpression>
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}