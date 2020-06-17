using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class ObserverProvider : ComponentOwnerBase<IObserverProvider>, IObserverProvider
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;

        private IMemberObserverProviderComponent[]? _memberObserverComponents;
        private IMemberPathProviderComponent[]? _memberPathComponents;
        private IMemberPathObserverProviderComponent[]? _memberPathObserverComponents;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObserverProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberObserverProviderComponent, ObserverProvider>((components, state, _) => state._memberObserverComponents = components, this);
            _componentTracker.AddListener<IMemberPathProviderComponent, ObserverProvider>((components, state, _) => state._memberPathComponents = components, this);
            _componentTracker.AddListener<IMemberPathObserverProviderComponent, ObserverProvider>((components, state, _) => state._memberPathObserverComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull] in TMember member, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberObserverComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberObserverComponents!.TryGetMemberObserver(type, member, metadata);
        }

        public IMemberPath? TryGetMemberPath<TPath>([DisallowNull] in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberPathComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberPathComponents!.TryGetMemberPath(path, metadata);
        }

        public IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberPathObserverComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberPathObserverComponents!.TryGetMemberPathObserver(target, request, metadata);
        }

        #endregion
    }
}