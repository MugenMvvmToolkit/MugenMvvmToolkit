using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingStateDispatcherComponent : IBindingStateDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IBinding, BindingLifecycleState, object, Type, IReadOnlyMetadataContext?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyMetadataContext? IBindingStateDispatcherComponent.OnLifecycleChanged<TState>(IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            return OnLifecycleChanged?.Invoke(binding, lifecycleState, state!, typeof(TState), metadata);
        }

        #endregion
    }
}