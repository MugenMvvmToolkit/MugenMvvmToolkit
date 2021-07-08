using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class EventMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        private static readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> MemberObserverHandler = TryObserve;

        private readonly IMemberManager? _memberManager;

        [Preserve(Conditional = true)]
        public EventMemberObserverProvider(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        public Func<Type, object, IReadOnlyMetadataContext?, IObservableMemberInfo?>? EventFinder { get; set; }

        public int Priority { get; init; } = ObservationComponentPriority.EventObserverProvider;

        public static IObservableMemberInfo? TryFindEventByMember(IMemberManager? memberManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            string memberName;
            EnumFlags<MemberFlags> flags;
            if (member is MemberInfo m)
            {
                flags = MemberFlags.All.ClearInstanceOrStaticFlags(m.GetAccessModifiers().HasFlag(MemberFlags.Static));
                memberName = m.Name;
            }
            else if (member is IMemberInfo memberInfo)
            {
                flags = MemberFlags.All.ClearInstanceOrStaticFlags(memberInfo.MemberFlags.HasFlag(MemberFlags.Static));
                memberName = memberInfo.Name;
            }
            else if (member is string st)
            {
                return memberManager.DefaultIfNull().TryGetMember(type, MemberType.Event, type.IsStatic() ? MemberFlags.StaticAll : MemberFlags.InstanceAll, st, metadata) as
                    IObservableMemberInfo;
            }
            else
                return null;

            memberManager = memberManager.DefaultIfNull();
            return memberManager.TryGetMember(type, MemberType.Event, flags, memberName + BindingInternalConstant.ChangedEventPostfix, metadata) as IObservableMemberInfo
                   ?? memberManager.TryGetMember(type, MemberType.Event, flags, memberName + BindingInternalConstant.ChangeEventPostfix, metadata) as IObservableMemberInfo;
        }

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is MemberInfo reflectionMember && reflectionMember.MemberType == MemberTypes.Event)
                return default;
            if (member is IMemberInfo memberInfo && memberInfo.MemberType == MemberType.Event)
                return default;

            var eventInfo = EventFinder == null ? TryFindEventByMember(_memberManager, type, member, metadata) : EventFinder.Invoke(type, member, metadata);
            if (eventInfo == null)
                return default;
            return new MemberObserver(MemberObserverHandler, eventInfo);
        }

        private static ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata) =>
            ((IObservableMemberInfo)member).TryObserve(target, listener, metadata);
    }
}