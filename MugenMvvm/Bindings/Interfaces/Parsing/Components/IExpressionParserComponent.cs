using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
        ItemOrIReadOnlyList<ExpressionParserResult> TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata);
    }
}