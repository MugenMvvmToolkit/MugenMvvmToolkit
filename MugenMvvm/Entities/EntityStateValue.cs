using System.Runtime.InteropServices;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct EntityStateValue
    {
        #region Fields

        public readonly object? Member;
        public readonly object? NewValue;
        public readonly object? OldValue;

        #endregion

        #region Constructors

        public EntityStateValue(object member, object? oldValue, object? newValue)
        {
            Should.NotBeNull(member, nameof(member));
            Member = member;
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Member == null;

        #endregion
    }
}