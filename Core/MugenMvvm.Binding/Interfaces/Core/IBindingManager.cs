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
        ItemOrList<IDataBindingExpression, IReadOnlyList<IDataBindingExpression>> BuildBindingExpressions<T>(ref T expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IDataBinding, IReadOnlyList<IDataBinding>> BuildBindings<T>(ref T expression, object target, in ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IDataBinding?, IReadOnlyList<IDataBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyMetadataContext OnLifecycleChanged(IDataBinding binding, DataBindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null);
    }
}