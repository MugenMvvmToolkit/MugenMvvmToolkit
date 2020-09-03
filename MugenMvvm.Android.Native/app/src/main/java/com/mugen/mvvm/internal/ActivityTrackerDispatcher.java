package com.mugen.mvvm.internal;

import android.app.Activity;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.views.ActivityExtensions;

public class ActivityTrackerDispatcher implements ILifecycleDispatcher {

    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        if ((lifecycle == LifecycleState.Create || lifecycle == LifecycleState.Resume) && target instanceof Activity)
            ActivityExtensions.setCurrentActivity((Activity) target);
        else if ((lifecycle == LifecycleState.Finish || lifecycle == LifecycleState.Destroy))
            ActivityExtensions.clearCurrentActivity((Activity) target);
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PreInitializer;
    }
}
