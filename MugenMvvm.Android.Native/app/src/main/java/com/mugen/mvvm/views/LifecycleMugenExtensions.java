package com.mugen.mvvm.views;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenInitializerBase;
import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;

import java.util.ArrayList;

public final class LifecycleMugenExtensions {
    private static boolean _isChangingInitLifecycleState;
    private static int _initLifecycleState;

    private LifecycleMugenExtensions() {
    }

    public static void setInitializationLifecycleState(int state, boolean changing) {
        _initLifecycleState = state;
        _isChangingInitLifecycleState = changing;
    }

    public static boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (_initLifecycleState == 0 || (_isChangingInitLifecycleState && lifecycle == _initLifecycleState))
            MugenInitializerBase.ensureInitialized();

        ArrayList<ILifecycleDispatcher> lifecycleDispatchers = MugenService.getLifecycleDispatchers();
        for (int i = 0; i < lifecycleDispatchers.size(); i++) {
            if (!lifecycleDispatchers.get(i).onLifecycleChanging(target, lifecycle, state))
                return false;
        }
        return true;
    }

    public static void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (_initLifecycleState == 0 || lifecycle == _initLifecycleState)
            MugenInitializerBase.ensureInitialized();

        ArrayList<ILifecycleDispatcher> lifecycleDispatchers = MugenService.getLifecycleDispatchers();
        for (int i = 0; i < lifecycleDispatchers.size(); i++)
            lifecycleDispatchers.get(i).onLifecycleChanged(target, lifecycle, state);
    }
}
