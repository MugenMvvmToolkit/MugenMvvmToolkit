package com.mugen.mvvm.constants;

public final class MugenInitializationFlags {
    public static final int NativeMode = 1;
    public static final int NoFragmentState = 1 << 1;
    public static final int RawViewTagModeDisabled = 1 << 2;
    public static final int NoAppState = 1 << 3;

    public static final int CompatLib = 1 << 4;
    public static final int MaterialLib = 1 << 5;
    public static final int RecyclerViewLib = 1 << 6;
    public static final int SwipeRefreshLib = 1 << 7;
    public static final int ViewPagerLib = 1 << 8;
    public static final int ViewPager2Lib = 1 << 9;

    private MugenInitializationFlags() {
    }
}
