using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public abstract class TestMemberInfoBase : IObservableMemberInfo
    {
        public Func<object?, IEventListener, IReadOnlyMetadataContext?, ActionToken>? TryObserve { get; set; }

        public string Name { get; set; } = default!;

        public Type DeclaringType { get; set; } = default!;

        public Type Type { get; set; } = default!;

        public object? UnderlyingMember { get; set; } = default!;

        public MemberType MemberType { get; set; } = default!;

        public EnumFlags<MemberFlags> AccessModifiers { get; set; }

        public override string ToString() => $"{Name} - {DeclaringType?.Name} {Type?.Name}";

        ActionToken IObservableMemberInfo.TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata) =>
            TryObserve?.Invoke(target, listener, metadata) ?? default;
    }
}