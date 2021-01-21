package com.mugen.mvvm.constants;

public final class MugenInitializationFlags {
    public static final int CompatLib = 1;
    public static final int MaterialLib = 1 << 1;
    public static final int RecyclerViewLib = 1 << 2;
    public static final int SwipeRefreshLib = 1 << 3;
    public static final int ViewPagerLib = 1 << 4;
    public static final int ViewPager2Lib = 1 << 5;

    public static final int NativeMode = 1 << 12;
    public static final int NoFragmentState = 1 << 13;
    public static final int RawViewTagModeDisabled = 1 << 14;
    public static final int NoAppState = 1 << 15;
    public static final int Debug = 1 << 16;

    private MugenInitializationFlags() {
    }
}
