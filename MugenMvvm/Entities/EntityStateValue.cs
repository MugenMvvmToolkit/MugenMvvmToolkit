using System.Runtime.InteropServices;

namespace MugenMvvm.Entities
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct EntityStateValue
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

        public bool IsEmpty => Member == null;
    }
}