using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class NameRequestMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NameRequestMemberManagerDecorator(int priority = MemberComponentPriority.RequestHandlerDecorator)
            : base(priority)
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags,
            object request, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is string name))
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);
            _members.Clear();
            Owner.GetComponents<IMemberProviderComponent>(metadata).TryAddMembers(memberManager, _members, type, name, memberTypes, metadata);
            if (_members.Count == 0)
                return default;
            return Components.TryGetMembers(memberManager, type, memberTypes, flags, _members, metadata);
        }

        #endregion
    }
}