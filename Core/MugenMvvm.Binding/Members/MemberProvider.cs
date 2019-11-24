using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MemberProvider : ComponentOwnerBase<IMemberProvider>, IMemberProvider, IComponentOwnerAddedCallback<IComponent<IMemberProvider>>, IComponentOwnerRemovedCallback<IComponent<IMemberProvider>>
    {
        #region Fields

        private ISelectorMemberProviderComponent[] _memberSelectors;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _memberSelectors = Default.EmptyArray<ISelectorMemberProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IMemberProvider>>.OnComponentAdded(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _memberSelectors, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IMemberProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberSelectors, component);
        }

        public IMemberInfo? GetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;

            var selectors = _memberSelectors;
            for (var i = 0; i < selectors.Length; i++)
            {
                var result = selectors[i].TryGetMember(type, name, memberTypes, flags, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public IReadOnlyList<IMemberInfo> GetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;

            var selectors = _memberSelectors;
            for (var i = 0; i < selectors.Length; i++)
            {
                var result = selectors[i].TryGetMembers(type, name, memberTypes, flags, metadata)!;
                if (result != null)
                    return result;
            }

            return Default.EmptyArray<IMemberInfo>();
        }

        #endregion
    }
}