using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TrackingEntity
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
    }
}