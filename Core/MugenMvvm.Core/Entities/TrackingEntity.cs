using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TrackingEntity
    {
        #region Fields

        public readonly object Entity;
        public readonly EntityState State;

        #endregion

        #region Constructors

        public TrackingEntity(object entity, EntityState state)
        {
            Should.NotBeNull(entity, nameof(entity));
            Should.NotBeNull(state, nameof(state));
            Entity = entity;
            State = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Entity == null;

        #endregion
    }
}