using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers.Components
{
    public class ObservableMemberBindingMemberObserverProviderComponent : IBindingMemberObserverProviderComponent, BindingMemberObserver.IHandler
    {
        #region Fields

        private readonly IBindingMemberProvider _memberProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObservableMemberBindingMemberObserverProviderComponent(IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, nameof(memberProvider));
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        public Func<Type, string, IReadOnlyMetadataContext?, IBindingMemberInfo?>? ObservableMemberFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        IDisposable? BindingMemberObserver.IHandler.TryObserve(object? target, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IBindingMemberInfo)member).TryObserve(target, listener, metadata);
        }

        public BindingMemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is string name)
            {
                var observableMember = TryGetObservableMember(type, name, metadata);
                if (observableMember != null)
                    return new BindingMemberObserver(this, observableMember);
            }

            return default;
        }

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        #endregion

        #region Methods

        private IBindingMemberInfo? TryGetObservableMember(Type type, string memberName, IReadOnlyMetadataContext? metadata)
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