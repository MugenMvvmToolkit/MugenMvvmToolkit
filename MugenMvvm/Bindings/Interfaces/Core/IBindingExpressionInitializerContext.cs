using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBindingExpressionInitializerContext : IMetadataOwner<IMetadataContext>
    {
        object Target { get; }

        object? Source { get; }

        IExpressionNode TargetExpression { get; set; }

        IExpressionNode? SourceExpression { get; set; }

        ItemOrList<IExpressionNode, IList<IExpressionNode>> ParameterExpressions { get; set; }

        IDictionary<string, object?> Components { get; }

        [return: MaybeNull]
        TValue TryGetParameterValue<TValue>(string parameterName, TValue defaultValue = default);
    }
}