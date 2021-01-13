using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBindingExpressionInitializerContext : IMetadataOwner<IMetadataContext>
    {
        object Target { get; }

        object? Source { get; }

        IExpressionNode TargetExpression { get; set; }

        IExpressionNode? SourceExpression { get; set; }

        ItemOrIReadOnlyList<IExpressionNode> ParameterExpressions { get; set; }

        IDictionary<string, object?> Components { get; }

        TValue? TryGetParameterValue<TValue>(string parameterName, TValue defaultValue = default);
    }
}