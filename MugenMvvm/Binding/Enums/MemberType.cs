using System;

namespace MugenMvvm.Bindings.Enums
{
    [Flags]
    public enum MemberType : byte
    {
        Accessor = 1,
        Method = 1 << 1,
        Event = 1 << 2,
        All = Accessor | Method | Event
    }
}