using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observation
{
    public sealed class ObservationManager : ComponentOwnerBase<IObservationManager>, IObservationManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;

        private IMemberObserverProviderComponent[]? _memberObserverComponents;
        private IMemberPathProviderComponent[]? _memberPathComponents;
        private IMemberPathObserverProviderComponent[]? _memberPathObserverComponents;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObservationManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberObserverProviderComponent, ObservationManager>((components, state, _) => state._memberObserverComponents = components, this);
            _componentTracker.AddListener<IMemberPathProviderComponent, ObservationManager>((components, state, _) => state._memberPathComponents = components, this);
            _componentTracker.AddListener<IMemberPathObserverProviderComponent, ObservationManager>((components, state, _) => state._memberPathObserverComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberObserverComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberObserverComponents!.TryGetMemberObserver(this, type, member, metadata);
        }

        public IMemberPath? TryGetMemberPath(object path, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberPathComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberPathComponents!.TryGetMemberPath(this, path, metadata);
        }

        public IMemberPathObserver? TryGetMemberPathObserver(object target, object request, IReadOnlyMetadataContext? metadata = null)
        {
            if (_memberPathObserverComponents == null)
                _componentTracker.Attach(this, metadata);
            return _memberPathObserverComponents!.TryGetMemberPathObserver(this, target, request, metadata);
        }

        #endregion
    }
}