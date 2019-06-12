using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public class ObservableMemberChildBindingObserverProvider : IChildBindingObserverProvider, IBindingMemberObserverCallback
    {
        #region Fields

        private readonly IBindingMemberProvider _memberProvider;

        #endregion

        #region Constructors

        public ObservableMemberChildBindingObserverProvider(IBindingMemberProvider memberProvider)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        public Func<Type, string, IReadOnlyMetadataContext, IBindingMemberInfo?>? ObservableMemberFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        public IDisposable TryObserve(object target, object member, IBindingEventListener listener, IReadOnlyMetadataContext metadata)
        {
            return ((IBindingMemberInfo) member).TryObserve(target, listener);
        }

        public bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            if (member is string name)
            {
                var observableMember = TryGetObservableMember(type, name, metadata);
                if (observableMember != null)
                {
                    observer = new BindingMemberObserver(observableMember, this);
                    return true;
                }
            }

            observer = default;
            return false;
        }

        #endregion

        #region Methods

        private IBindingMemberInfo? TryGetObservableMember(Type type, string memberName, IReadOnlyMetadataContext metadata)
        {
            if (ObservableMemberFinder != null)
                return ObservableMemberFinder(type, memberName, metadata);

            var member = _memberProvider.GetMember(type, memberName + BindingInternalConstants.ChangedEventPostfix, metadata);
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = _memberProvider.GetMember(type, memberName + "Change", metadata);
            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        #endregion
    }
}