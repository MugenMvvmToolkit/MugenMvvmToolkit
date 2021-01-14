using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingLifecycleListener : IBindingLifecycleListener, IHasPriority
    {
        private readonly IBindingManager? _bindingManager;

        public TestBindingLifecycleListener(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        public Action<IBinding, BindingLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IBindingLifecycleListener.OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            _bindingManager?.ShouldEqual(bindingManager);
            OnLifecycleChanged?.Invoke(binding, lifecycleState, state, metadata);
        }
    }
}