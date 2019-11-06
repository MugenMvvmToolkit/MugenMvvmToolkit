using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum MemberType : byte
    {
        Field = 1,
        Property = 1 << 1,
        Method = 1 << 2,
        Event = 1 << 3,
        All = Field | Property | Method | Event
    }
}