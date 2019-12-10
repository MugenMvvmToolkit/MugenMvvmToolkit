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
        ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata = null);
    }
}