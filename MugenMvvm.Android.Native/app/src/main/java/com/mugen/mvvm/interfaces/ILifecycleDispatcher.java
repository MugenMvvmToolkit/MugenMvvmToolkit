package com.mugen.mvvm.interfaces;

public interface ILifecycleDispatcher {
    boolean onLifecycleChanging(Object target, int lifecycle, Object state);

    void onLifecycleChanged(Object target, int lifecycle, Object state);
}
