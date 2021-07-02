using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TrackingEntity : IEquatable<TrackingEntity>
    {
        public readonly object? Entity;
        public readonly EntityState? State;

        public TrackingEntity(object entity, EntityState state)
        {
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(state, nameof(state));
            Entity = entity;
            State = state;
        }

        [MemberNotNullWhen(false, nameof(Entity), nameof(State))]
        public bool IsEmpty => Entity == null;

        public bool Equals(TrackingEntity other) => Equals(Entity, other.Entity) && Equals(State, other.State);

        public override bool Equals(object? obj) => obj is TrackingEntity other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Entity, State);
    }
}