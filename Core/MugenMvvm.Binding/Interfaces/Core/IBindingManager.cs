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
        ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<T>(in T expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildBinding<T>(in T expression, object target, ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding?, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null);
    }
}