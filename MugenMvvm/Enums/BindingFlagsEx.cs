using System.Reflection;

namespace MugenMvvm.Enums
{
    internal static class BindingFlagsEx
    {
        public const BindingFlags All = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;
        public const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
        public const BindingFlags StaticPublic = BindingFlags.Static | BindingFlags.Public;
        public const BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;
        public const BindingFlags StaticOnly = StaticPublic | BindingFlags.NonPublic;
        public const BindingFlags InstanceOnly = InstancePublic | BindingFlags.NonPublic;
    }
}