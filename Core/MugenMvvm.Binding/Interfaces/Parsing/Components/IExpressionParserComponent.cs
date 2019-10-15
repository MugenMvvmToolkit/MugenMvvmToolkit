using System.Collections.Generic;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}