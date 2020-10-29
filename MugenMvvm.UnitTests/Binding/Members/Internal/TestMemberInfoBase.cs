using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public abstract class TestMemberInfoBase : IObservableMemberInfo
    {
        #region Properties

        public string Name { get; set; } = default!;

        public Type DeclaringType { get; set; } = default!;

        public Type Type { get; set; } = default!;

        public object? UnderlyingMember { get; set; } = default!;

        public MemberType MemberType { get; set; } = default!;

        public MemberFlags AccessModifiers { get; set; } = default!;

        public Func<object?, IEventListener, IReadOnlyMetadataContext?, ActionToken>? TryObserve { get; set; }

        #endregion

        #region Implementation of interfaces

        ActionToken IObservableMemberInfo.TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata) => TryObserve?.Invoke(target, listener, metadata) ?? default;

        #endregion

        #region Methods

        public override string ToString() => $"{Name} - {DeclaringType?.Name} {Type?.Name}";

        #endregion
    }
}