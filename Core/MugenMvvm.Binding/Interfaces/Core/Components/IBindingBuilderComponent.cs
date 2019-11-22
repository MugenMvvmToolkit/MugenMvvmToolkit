using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingBuilderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBinding, IReadOnlyList<IBinding>> TryBuildBinding<TExpression>(in TExpression expression, object target, ItemOrList<object?, IReadOnlyList<object?>> sources,
            IReadOnlyMetadataContext? metadata);
    }
}