using System;

namespace MugenMvvm.Enums
{
    [Flags]
    public enum MemberFlags
    {
        Static = 1,
        Instance = 2,
        Public = 4,
        NonPublic = 8,
        All = Static | Instance | Public | NonPublic,
        StaticOnly = Static | Public | NonPublic,
        InstanceOnly = Instance | Public | NonPublic,
        InstancePublic = Instance | Public,
        StaticPublic = Static | Public
    }
}