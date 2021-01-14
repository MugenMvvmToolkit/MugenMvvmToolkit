using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Members
{
    public sealed class MemberManager : ComponentOwnerBase<IMemberManager>, IMemberManager, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        private readonly ComponentTracker _componentTracker;
        private ItemOrArray<IMemberManagerComponent> _components;

        [Preserve(Conditional = true)]
        public MemberManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberManagerComponent, MemberManager>((components, state, _) => state._components = components, this);
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags, object request,
            IReadOnlyMetadataContext? metadata = null)
            => _components.TryGetMembers(this, type, memberTypes, flags, request, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);
    }
}