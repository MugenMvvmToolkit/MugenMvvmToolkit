using System;
using System.Diagnostics.CodeAnalysis;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class EventMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberManager? _memberManager;
        private static readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> MemberObserverHandler = TryObserve;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public EventMemberObserverProvider(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.Event;

        public Func<Type, object, IReadOnlyMetadataContext?, IEventInfo?>? EventFinder { get; set; }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull] in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TMember>())
                return default;
            if (member is MemberInfo reflectionMember && reflectionMember.MemberType == MemberTypes.Event)
                return default;
            if (member is IMemberInfo memberInfo && memberInfo.MemberType == MemberType.Event)
                return default;

            var eventInfo = EventFinder == null ? TryFindEventByMember(_memberManager, type, member, metadata) : EventFinder.Invoke(type, member, metadata);
            if (eventInfo == null)
                return default;
            return new MemberObserver(MemberObserverHandler, eventInfo);
        }

        #endregion

        #region Methods

        public static IEventInfo? TryFindEventByMember(IMemberManager? memberManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
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
            else if (member is string st)
                return memberManager.DefaultIfNull().GetMember(type, MemberType.Event, type.IsStatic() ? MemberFlags.StaticPublic : MemberFlags.InstancePublic, st, metadata) as IEventInfo;
            else
                return null;

            memberManager = memberManager.DefaultIfNull();
            return memberManager.GetMember(type, MemberType.Event, flags, memberName + BindingInternalConstant.ChangedEventPostfix, metadata) as IEventInfo
                   ?? memberManager.GetMember(type, MemberType.Event, flags, memberName + BindingInternalConstant.ChangeEventPostfix, metadata) as IEventInfo;
        }

        private static ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return ((IEventInfo) member).TrySubscribe(target, listener, metadata);
        }

        #endregion
    }
}