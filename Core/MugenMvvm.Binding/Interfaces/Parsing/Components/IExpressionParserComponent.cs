using System.Collections.Generic;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
    }

    public interface IExpressionParserComponent<TExpression> : IExpressionParserComponent
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}