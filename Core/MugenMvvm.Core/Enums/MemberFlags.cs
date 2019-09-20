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
        All = Static | Instance | Public | NonPublic,
        StaticOnly = Static | Public | NonPublic,
        InstanceOnly = Instance | Public | NonPublic,
        InstancePublic = Instance | Public,
        StaticPublic = Static | Public
    }
}