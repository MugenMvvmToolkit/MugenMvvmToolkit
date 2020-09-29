using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class NameRequestMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NameRequestMemberManagerDecorator(int priority = MemberComponentPriority.RequestHandler)
            : base(priority)
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, object request, IReadOnlyMetadataContext? metadata)
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