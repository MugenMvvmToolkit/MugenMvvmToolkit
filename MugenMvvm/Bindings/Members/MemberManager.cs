using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class MemberManager : ComponentOwnerBase<IMemberManager>, IMemberManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IMemberManagerComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IMemberManagerComponent, MemberManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags, object request, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryGetMembers(this, type, memberTypes, flags, request, metadata);
        }

        #endregion
    }
}