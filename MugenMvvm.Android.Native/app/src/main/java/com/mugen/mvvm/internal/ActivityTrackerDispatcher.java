package com.mugen.mvvm.internal;

import android.app.Activity;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.views.ActivityMugenExtensions;

public class ActivityTrackerDispatcher implements ILifecycleDispatcher {

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state, boolean cancelable) {
        if ((lifecycle == LifecycleState.Create || lifecycle == LifecycleState.Resume) && target instanceof Activity)
            ActivityMugenExtensions.setCurrentActivity((Activity) target);
        else if ((lifecycle == LifecycleState.Finish || lifecycle == LifecycleState.Destroy) && target instanceof Activity)
            ActivityMugenExtensions.clearCurrentActivity((Activity) target);
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstant.PreInitializer;
    }
}
