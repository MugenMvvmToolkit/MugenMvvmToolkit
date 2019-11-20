using System;

namespace MugenMvvm.Binding.Enums
{
    [Flags]
    public enum MemberFlags : byte
    {
        Public = 1,
        NonPublic = 1 << 1,

        Static = 1 << 2,
        Instance = 1 << 3,
        Attached = 1 << 4,
        Dynamic = Instance | 1 << 5,
        Extension = Instance | 1 << 6,

        InstancePublic = Instance | Public,
        InstanceNonPublic = Instance | NonPublic,

        StaticPublic = Static | Public,
        StaticNonPublic = Static | NonPublic,

        StaticOnly = StaticPublic | StaticNonPublic,
        InstanceOnly = InstancePublic | InstanceNonPublic,

        All = Static | Instance | Public | NonPublic | Attached | Dynamic | Extension
    }
}