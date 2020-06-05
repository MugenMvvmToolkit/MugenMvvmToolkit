using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public sealed class DelegateEventMemberInfo<TTarget, TState> : IEventInfo, INotifiableMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly RaiseDelegate? _raise;
        private readonly TrySubscribeDelegate _trySubscribe;
        public readonly TState State;

        #endregion

        #region Constructors

        public DelegateEventMemberInfo(string name, Type declaringType, TState state, TrySubscribeDelegate trySubscribe,
            RaiseDelegate? raise = null, Type? eventType = null, MemberFlags accessModifiers = MemberFlags.InstancePublic)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(trySubscribe, nameof(trySubscribe));
            _trySubscribe = trySubscribe;
            _raise = raise;
            Name = name;
            DeclaringType = declaringType;
            State = state;
            Type = eventType ?? typeof(EventHandler);
            AccessModifiers = accessModifiers | MemberFlags.Attached;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Event;

        public MemberFlags AccessModifiers { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TrySubscribe(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return _trySubscribe(this, (TTarget)target!, listener, metadata);
        }

        public void Raise(object? target, object? message, IReadOnlyMetadataContext? metadata = null)
        {
            _raise?.Invoke(this, (TTarget)target!, metadata, metadata);
        }

        #endregion

        #region Nested types

        public delegate void RaiseDelegate(DelegateEventMemberInfo<TTarget, TState> eventMember, TTarget target, object? message, IReadOnlyMetadataContext? metadata);

        public delegate ActionToken TrySubscribeDelegate(DelegateEventMemberInfo<TTarget, TState> eventMember, TTarget target, IEventListener listener, IReadOnlyMetadataContext? metadata);

        #endregion
    }
}