package com.mugen.mvvm.constants;

public abstract class MugenInitializationFlags {

    public static final int NativeMode = 1;
    public static final int FragmentStateDisabled = 1 << 1;
    public static final int RawViewTagModeDisabled = 1 << 2;
    public static final int CompatLib = 1 << 3;
    public static final int MaterialLib = 1 << 4;
    public static final int RecyclerViewLib = 1 << 5;
    public static final int SwipeRefreshLib = 1 << 6;
    public static final int ViewPagerLib = 1 << 7;
    public static final int ViewPager2Lib = 1 << 8;

    private MugenInitializationFlags() {
    }
}
