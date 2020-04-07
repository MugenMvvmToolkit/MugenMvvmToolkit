using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public class MemberManagerComponent : AttachableComponentBase<IMemberManager>, IMemberManagerComponent, IHasPriority
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        public MemberManagerComponent()
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Manager;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) == typeof(MemberManagerRequest))
            {
                var r = MugenExtensions.CastGeneric<TRequest, MemberManagerRequest>(request);
                _members.Clear();
                Owner.GetComponents<IMemberProviderComponent>(metadata).TryAddMembers(_members, r.Type, r.Name, metadata);
                return Owner.GetComponents<IMemberSelectorComponent>(metadata).TrySelectMembers(_members, r.Type, r.MemberTypes, r.Flags, metadata);
            }

            return default;
        }

        #endregion
    }
}