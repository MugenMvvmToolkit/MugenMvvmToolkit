using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public class EventMemberObserverProviderComponent : IMemberObserverProviderComponent<PropertyInfo>, IMemberObserverProviderComponent<MethodInfo>, MemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventMemberObserverProviderComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        public Func<Type, string, IReadOnlyMetadataContext?, IBindingEventInfo?>? EventFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        IDisposable? MemberObserver.IHandler.TryObserve(object? source, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IBindingEventInfo)member).TrySubscribe(source, listener, metadata);
        }

        public MemberObserver TryGetMemberObserver(Type type, in MethodInfo member, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, member.GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(this, observableMember);

            return default;
        }

        public MemberObserver TryGetMemberObserver(Type type, in PropertyInfo member, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, (member.GetGetMethodUnified(true) ?? member.GetSetMethodUnified(true)).GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(this, observableMember);

            return default;
        }

        #endregion

        #region Methods

        private IBindingEventInfo? TryGetEvent(Type type, string memberName, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            if (EventFinder != null)
                return EventFinder(type, memberName, metadata);

            var provider = _memberProvider.ServiceIfNull();
            return provider.GetMember(type, memberName + BindingInternalConstants.ChangedEventPostfix, BindingMemberType.Event, flags, metadata) as IBindingEventInfo
                   ?? provider.GetMember(type, memberName + "Change", BindingMemberType.Event, flags, metadata) as IBindingEventInfo;
        }

        #endregion
    }
}