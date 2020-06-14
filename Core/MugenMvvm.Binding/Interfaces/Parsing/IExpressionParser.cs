using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionParser : IComponentOwner<IExpressionParser>, IComponent<IMugenApplication>
    {
        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse<TExpression>([DisallowNull] in TExpression expression, IReadOnlyMetadataContext? metadata = null);
    }
}