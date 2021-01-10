using System.Collections.Generic;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
        ItemOrIReadOnlyList<ExpressionParserResult> TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata);
    }
}