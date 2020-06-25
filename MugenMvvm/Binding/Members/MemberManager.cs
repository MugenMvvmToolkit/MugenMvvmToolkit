using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public sealed class MemberManager : ComponentOwnerBase<IMemberManager>, IMemberManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IMemberManagerComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberManagerComponent, MemberManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryGetMembers(type, memberTypes, flags, request, metadata);
        }

        #endregion
    }
}