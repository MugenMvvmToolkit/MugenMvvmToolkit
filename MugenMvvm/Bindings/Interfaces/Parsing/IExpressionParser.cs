using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface IExpressionParser : IComponentOwner<IExpressionParser>
    {
        ItemOrIReadOnlyList<ExpressionParserResult> TryParse(object expression, IReadOnlyMetadataContext? metadata = null);
    }
}