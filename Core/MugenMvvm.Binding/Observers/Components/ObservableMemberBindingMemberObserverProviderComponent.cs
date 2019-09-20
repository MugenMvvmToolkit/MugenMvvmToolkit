using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public class ObservableMemberBindingMemberObserverProviderComponent : IBindingMemberObserverProviderComponent<string>, BindingMemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IBindingMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObservableMemberBindingMemberObserverProviderComponent(IBindingMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        public Func<Type, string, IReadOnlyMetadataContext?, IObservableBindingMemberInfo?>? ObservableMemberFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        public BindingMemberObserver TryGetMemberObserver(Type type, in string member, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetObservableMember(type, member, metadata);
            if (observableMember != null)
                return new BindingMemberObserver(this, observableMember);

            return default;
        }

        IDisposable? BindingMemberObserver.IHandler.TryObserve(object? source, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IObservableBindingMemberInfo)member).TryObserve(source, listener, metadata);
        }

        #endregion

        #region Methods

        private IObservableBindingMemberInfo? TryGetObservableMember(Type type, string memberName, IReadOnlyMetadataContext? metadata)
        {
            if (ObservableMemberFinder != null)
                return ObservableMemberFinder(type, memberName, metadata);

            var provider = _memberProvider.ServiceIfNull();
            var member = provider.GetMember(type, memberName + BindingInternalConstants.ChangedEventPostfix, metadata) as IObservableBindingMemberInfo;
            if (member == null || member.MemberType != BindingMemberType.Event)
                member = provider.GetMember(type, memberName + "Change", metadata) as IObservableBindingMemberInfo;
            if (member == null || member.MemberType != BindingMemberType.Event)
                return null;
            return member;
        }

        #endregion
    }
}