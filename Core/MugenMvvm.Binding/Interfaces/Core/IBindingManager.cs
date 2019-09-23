using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingManager : IComponentOwner<IBindingManager>, IComponent<IMugenApplication>
    {
        ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpressions<T>(in T expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildBindings<T>(in T expression, object target, in ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding?, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, DataBindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null);
    }
}