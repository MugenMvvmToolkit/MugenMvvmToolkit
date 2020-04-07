using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberManager? _memberManager;
        private static readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> MemberObserverHandler = TryObserve;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventMemberObserverProviderComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.Event;

        public Func<Type, string, IReadOnlyMetadataContext?, IEventInfo?>? EventFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TMember>())
            {
                if (typeof(TMember) == typeof(MemberObserverRequest))
                {
                    var request = MugenExtensions.CastGeneric<TMember, MemberObserverRequest>(member);
                    string? memberName;
                    switch (request.ReflectionMember)
                    {
                        case PropertyInfo p:
                            if (request.MemberInfo == null)
                                return TryGetMemberObserver(p, type, metadata);
                            memberName = p.Name;
                            break;
                        case MethodInfo m:
                            if (request.MemberInfo == null)
                                return TryGetMemberObserver(m, type, metadata);
                            memberName = m.Name;
                            break;
                        default:
                            return default;
                    }

                    var observableMember = TryGetEvent(type, memberName, request.MemberInfo.AccessModifiers, metadata);
                    if (observableMember != null)
                        return new MemberObserver(MemberObserverHandler, observableMember);
                }

                return default;
            }

            if (member is MethodInfo method)
                return TryGetMemberObserver(method, type, metadata);
            if (member is PropertyInfo propertyInfo)
                return TryGetMemberObserver(propertyInfo, type, metadata);
            return default;
        }

        #endregion

        #region Methods

        private static ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IEventInfo) member).TrySubscribe(target, listener, metadata);
        }

        private MemberObserver TryGetMemberObserver(MethodInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, member.GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(MemberObserverHandler, observableMember);

            return default;
        }

        private MemberObserver TryGetMemberObserver(PropertyInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            var observableMember = TryGetEvent(type, member.Name, member.GetAccessModifiers(), metadata);
            if (observableMember != null)
                return new MemberObserver(MemberObserverHandler, observableMember);

            return default;
        }

        private IEventInfo? TryGetEvent(Type type, string memberName, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            if (EventFinder != null)
                return EventFinder(type, memberName, metadata);

            var provider = _memberManager.DefaultIfNull();
            return provider.GetMember(type, memberName + BindingInternalConstant.ChangedEventPostfix, MemberType.Event, flags, metadata) as IEventInfo
                   ?? provider.GetMember(type, memberName + "Change", MemberType.Event, flags, metadata) as IEventInfo;
        }

        #endregion
    }
}