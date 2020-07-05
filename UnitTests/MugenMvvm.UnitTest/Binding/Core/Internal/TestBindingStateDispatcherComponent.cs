using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingStateDispatcherComponent : IBindingLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBindingManager, IBinding, BindingLifecycleState, object, Type, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingLifecycleDispatcherComponent.OnLifecycleChanged<TState>(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged?.Invoke(bindingManager, binding, lifecycleState, state!, typeof(TState), metadata);
        }

        #endregion
    }
}