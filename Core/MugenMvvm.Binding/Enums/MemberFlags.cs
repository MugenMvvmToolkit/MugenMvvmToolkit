using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum MemberFlags : byte
    {
        Static = 1,
        Instance = 1 << 1,
        Public = 1 << 2,
        NonPublic = 1 << 3,
        Attached = 1 << 4,
        Dynamic = 1 << 5,
        Extension = 1 << 6,

        All = Static | Instance | Public | NonPublic | Attached | Dynamic | Extension,
        InstancePublic = Instance | Public,
        InstanceNonPublic = Instance | NonPublic,
        StaticPublic = Static | Public,
        StaticNonPublic = Static | NonPublic,
        StaticOnly = StaticPublic | NonPublic,
        InstanceOnly = InstancePublic | NonPublic
    }
}