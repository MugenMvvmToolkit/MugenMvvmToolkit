package com.mugen.mvvm.views;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;

import java.util.ArrayList;

public final class LifecycleMugenExtensions {
    private LifecycleMugenExtensions() {
    }

    public static boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        ArrayList<ILifecycleDispatcher> lifecycleDispatchers = MugenService.getLifecycleDispatchers();
        for (int i = 0; i < lifecycleDispatchers.size(); i++) {
            if (!lifecycleDispatchers.get(i).onLifecycleChanging(target, lifecycle, state))
                return false;
        }
        return true;
    }

    public static void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        ArrayList<ILifecycleDispatcher> lifecycleDispatchers = MugenService.getLifecycleDispatchers();
        for (int i = 0; i < lifecycleDispatchers.size(); i++)
            lifecycleDispatchers.get(i).onLifecycleChanged(target, lifecycle, state);
    }
}
