using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingLifecycleListener : IBindingLifecycleListener, IHasPriority
    {
        public Action<IBindingManager, IBinding, BindingLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IBindingLifecycleListener.OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata) =>
            OnLifecycleChanged?.Invoke(bindingManager, binding, lifecycleState, state, metadata);
    }
}