using System;

namespace MugenMvvm.Enums
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

        All = Static | Instance | Public | NonPublic | Attached | Dynamic,
        InstancePublic = Instance | Public,
        InstanceNonPublic = Instance | NonPublic,
        StaticPublic = Static | Public,
        StaticNonPublic = Static | NonPublic,
        StaticOnly = StaticPublic | NonPublic,
        InstanceOnly = InstancePublic | NonPublic
    }
}