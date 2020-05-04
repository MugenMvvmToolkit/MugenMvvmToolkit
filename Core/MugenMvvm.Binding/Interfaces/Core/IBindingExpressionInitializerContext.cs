using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingExpressionInitializerContext : IMetadataOwner<IMetadataContext>
    {
        object Target { get; }

        object? Source { get; }

        IExpressionNode TargetExpression { get; set; }

        IExpressionNode? SourceExpression { get; set; }

        ItemOrList<IExpressionNode, IList<IExpressionNode>> Parameters { get; set; }

        IDictionary<string, object?> BindingComponents { get; }

        TValue TryGetParameterValue<TValue>(string parameterName, TValue defaultValue = default);
    }
}