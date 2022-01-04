using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Bindings.Members.Components
{
    public class SynchronizedMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IMemberProviderComponent,
        IHasCacheComponent<IMemberManager>, IComponentCollectionDecorator<IHasCacheComponent<IMemberManager>>,
        IComponentCollectionDecorator<IMemberProviderComponent>, ISynchronizedComponent<IMemberManager>
    {
        private ItemOrArray<IHasCacheComponent<IMemberManager>> _cacheComponents;
        private ItemOrArray<IMemberProviderComponent> _memberProviders;
        private readonly object _syncRoot;

        public SynchronizedMemberManagerDecorator(object? syncRoot = null, int priority = ComponentPriority.Synchronizer) : base(priority)
        {
            _syncRoot = syncRoot ?? this;
        }

        public object SyncRoot => _syncRoot;

        void IComponentCollectionDecorator<IHasCacheComponent<IMemberManager>>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IHasCacheComponent<IMemberManager>> components, IReadOnlyMetadataContext? metadata) =>
            _cacheComponents = this.Decorate(ref components);

        void IComponentCollectionDecorator<IMemberProviderComponent>.Decorate(IComponentCollection collection, ref ItemOrListEditor<IMemberProviderComponent> components,
            IReadOnlyMetadataContext? metadata) =>
            _memberProviders = this.Decorate(ref components);

        void IHasCacheComponent<IMemberManager>.Invalidate(IMemberManager owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                _cacheComponents.Invalidate(owner, state, metadata);
            }
        }

        ItemOrIReadOnlyList<IMemberInfo> IMemberManagerComponent.TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes,
            EnumFlags<MemberFlags> flags, object request, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);
            }
        }

        ItemOrIReadOnlyList<IMemberInfo> IMemberProviderComponent.TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return _memberProviders.TryGetMembers(memberManager, type, name, memberTypes, metadata);
            }
        }
    }
}