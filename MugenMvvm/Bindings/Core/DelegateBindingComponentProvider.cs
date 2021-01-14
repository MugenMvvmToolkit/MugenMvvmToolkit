using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    public sealed class DelegateBindingComponentProvider<TState> : IBindingComponentProvider
    {
        private readonly Func<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?> _componentFactory;
        private readonly TState _state;

        public DelegateBindingComponentProvider(Func<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?> componentFactory, TState state)
        {
            Should.NotBeNull(componentFactory, nameof(componentFactory));
            _componentFactory = componentFactory;
            _state = state;
        }

        public IComponent<IBinding>? TryGetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata) =>
            _componentFactory(_state, binding, target, source, metadata);
    }
}