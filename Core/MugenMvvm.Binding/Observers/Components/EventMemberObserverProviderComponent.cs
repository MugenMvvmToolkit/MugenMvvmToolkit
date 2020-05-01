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

        public Func<Type, object, IReadOnlyMetadataContext?, IEventInfo?>? EventFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TMember>())
                return default;
            if (member is MemberInfo reflectionMember && reflectionMember.MemberType != MemberTypes.Event)
                return TryGetMemberObserverInternal(type, reflectionMember, metadata);
            if (member is IMemberInfo memberInfo && memberInfo.MemberType != MemberType.Event)
                return TryGetMemberObserverInternal(type, memberInfo, metadata);
            return default;
        }

        #endregion

        #region Methods

        private static ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IEventInfo) member).TrySubscribe(target, listener, metadata);
        }

        private MemberObserver TryGetMemberObserverInternal(Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            IEventInfo? eventInfo;
            if (EventFinder == null)
            {
                string memberName;
                MemberFlags flags;
                if (member is MemberInfo m)
                {
                    flags = m.GetAccessModifiers();
                    memberName = m.Name;
                }
                else if (member is IMemberInfo memberInfo)
                {
                    flags = memberInfo.AccessModifiers;
                    memberName = memberInfo.Name;
                }
                else
                    return default;

                var manager = _memberManager.DefaultIfNull();
                eventInfo = manager.GetMember(type, memberName + BindingInternalConstant.ChangedEventPostfix, MemberType.Event, flags, metadata) as IEventInfo
                            ?? manager.GetMember(type, memberName + BindingInternalConstant.ChangeEventPostfix, MemberType.Event, flags, metadata) as IEventInfo;
            }
            else
                eventInfo = EventFinder(type, member, metadata);

            if (eventInfo == null)
                return default;
            return new MemberObserver(MemberObserverHandler, eventInfo);
        }

        #endregion
    }
}