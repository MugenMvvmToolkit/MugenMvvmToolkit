using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager
    {
        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IBindingExpressionBuilderComponent>(metadata).TryBuildBindingExpression(expression, metadata);
            if (result.IsNullOrEmpty())
                BindingExceptionManager.ThrowCannotParseExpression(expression);
            return result;
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            return GetComponents<IBindingHolderComponent>(metadata).TryGetBindings(target, path, metadata);
        }

        public IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            return GetComponents<IBindingStateDispatcherComponent>(metadata).OnLifecycleChanged(binding, lifecycleState, metadata).DefaultIfNull();
        }

        #endregion
    }
}