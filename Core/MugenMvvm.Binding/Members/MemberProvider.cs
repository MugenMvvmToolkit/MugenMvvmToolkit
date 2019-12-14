using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MemberProvider : ComponentOwnerBase<IMemberProvider>, IMemberProvider
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IMemberProviderComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberProviderComponent, MemberProvider>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public IMemberInfo? GetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null)
        {
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            _components!.TryGetMember(type, name, memberTypes, flags, metadata, out var result);
            return result;
        }

        public IReadOnlyList<IMemberInfo> GetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags,
            IReadOnlyMetadataContext? metadata = null)
        {
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryGetMembers(type, name, memberTypes, flags, metadata) ?? Default.EmptyArray<IMemberInfo>();
        }

        #endregion
    }
}