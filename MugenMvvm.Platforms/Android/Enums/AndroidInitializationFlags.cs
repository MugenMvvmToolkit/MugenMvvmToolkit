using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Android.Enums
{
    public class AndroidInitializationFlags : FlagsEnumBase<AndroidInitializationFlags, int>
    {
        #region Fields

        public static readonly AndroidInitializationFlags NativeMode = new(NativeInitializationFlags.NativeMode);
        public static readonly AndroidInitializationFlags FragmentStateDisabled = new(NativeInitializationFlags.FragmentStateDisabled);
        public static readonly AndroidInitializationFlags RawViewTagModeDisabled = new(NativeInitializationFlags.RawViewTagModeDisabled);

        public static readonly AndroidInitializationFlags CompatLib = new(NativeInitializationFlags.CompatLib);
        public static readonly AndroidInitializationFlags MaterialLib = new(NativeInitializationFlags.MaterialLib);
        public static readonly AndroidInitializationFlags RecyclerViewLib = new(NativeInitializationFlags.RecyclerViewLib);
        public static readonly AndroidInitializationFlags SwipeRefreshLib = new(NativeInitializationFlags.SwipeRefreshLib);
        public static readonly AndroidInitializationFlags ViewPagerLib = new(NativeInitializationFlags.ViewPagerLib);
        public static readonly AndroidInitializationFlags ViewPager2Lib = new(NativeInitializationFlags.ViewPager2Lib);

        public static readonly EnumFlags<AndroidInitializationFlags> AllSupportLibs = CompatLib | MaterialLib | RecyclerViewLib | SwipeRefreshLib | ViewPagerLib | ViewPager2Lib;

        #endregion

        #region Constructors

        public AndroidInitializationFlags(int value, string? name = null, long? flag = null) : base(value, name, flag)
        {
        }

        #endregion
    }
}