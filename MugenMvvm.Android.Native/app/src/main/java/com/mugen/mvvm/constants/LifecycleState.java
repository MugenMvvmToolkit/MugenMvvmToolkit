package com.mugen.mvvm.constants;

public abstract class LifecycleState {
    public static final int Finish = 1;
    public static final int FinishAfterTransition = 2;
    public static final int BackPressed = 3;
    public static final int NewIntent = 4;
    public static final int ConfigurationChanged = 5;
    public static final int Create = 6;
    public static final int Destroy = 7;
    public static final int Pause = 8;
    public static final int Restart = 9;
    public static final int Resume = 10;
    public static final int SaveState = 11;
    public static final int Start = 12;
    public static final int Stop = 13;
    public static final int PostCreate = 14;
}