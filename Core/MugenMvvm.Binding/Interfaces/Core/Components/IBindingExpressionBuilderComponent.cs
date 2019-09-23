using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionBuilderComponent : IComponent<IBindingManager>
    {
    }

    public interface IBindingExpressionBuilderComponent<TExpression> : IBindingExpressionBuilderComponent
    {
        IReadOnlyList<IBindingExpression> TryBuildBindingExpressions(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}