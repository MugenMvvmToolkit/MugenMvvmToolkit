using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct EntityStateValue : IEquatable<EntityStateValue>
    {
        public readonly object? Member;
        public readonly object? NewValue;
        public readonly object? OldValue;

        public EntityStateValue(object member, object? oldValue, object? newValue)
        {
            Should.NotBeNull(member, nameof(member));
            Member = member;
            OldValue = oldValue;
            NewValue = newValue;
        }

        [MemberNotNullWhen(false, nameof(Member))]
        public bool IsEmpty => Member == null;

        public bool Equals(EntityStateValue other) => Equals(Member, other.Member) && Equals(NewValue, other.NewValue) && Equals(OldValue, other.OldValue);

        public override bool Equals(object? obj) => obj is EntityStateValue other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Member, NewValue, OldValue);
    }
}