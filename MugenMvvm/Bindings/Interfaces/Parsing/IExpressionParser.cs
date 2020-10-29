using System.Collections.Generic;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface IExpressionParser : IComponentOwner<IExpressionParser>
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(object expression, IReadOnlyMetadataContext? metadata = null);
    }
}