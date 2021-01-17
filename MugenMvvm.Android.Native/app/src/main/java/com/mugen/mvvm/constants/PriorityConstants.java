package com.mugen.mvvm.constants;

public final class PriorityConstants {
    public static final int AppStateDispatcher = 1000;
    public static final int PreInitializer = 100;
    public static final int Default = 0;
    public static final int PostInitializer = -100;

    private PriorityConstants() {
    }
}
