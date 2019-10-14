using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionBuilderComponent : IComponent<IBindingManager>
    {
    }

    public interface IBindingExpressionBuilderComponent<TExpression> : IBindingExpressionBuilderComponent
    {
        ItemOrList<IBindingExpression?, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}