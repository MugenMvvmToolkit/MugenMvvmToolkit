using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionParser : IComponentOwner<IExpressionParser>, IComponent<IBindingManager>
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null);
    }
}