using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class DelegateBindingComponentProvider<TState> : IBindingComponentProvider
    {
        #region Fields

        private readonly FuncIn<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?> _componentFactory;
        private readonly TState _state;

        #endregion

        #region Constructors

        public DelegateBindingComponentProvider(FuncIn<TState, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?> componentFactory, in TState state)
        {
            Should.NotBeNull(componentFactory, nameof(componentFactory));
            _componentFactory = componentFactory;
            _state = state;
        }

        #endregion

        #region Implementation of interfaces

        public IComponent<IBinding>? TryGetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return _componentFactory(_state, binding, target, source, metadata);
        }

        #endregion
    }
}