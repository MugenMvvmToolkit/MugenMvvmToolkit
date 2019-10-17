using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingComponentProviderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>> TryGetComponentBuilders(IBinding binding,
            object target, object? source, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata);
    }
}