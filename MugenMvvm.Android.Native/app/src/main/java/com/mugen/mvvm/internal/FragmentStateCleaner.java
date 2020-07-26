package com.mugen.mvvm.internal;

import android.app.Activity;
import android.os.Bundle;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;

public class FragmentStateCleaner implements ILifecycleDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        if (lifecycle == LifecycleState.Create && target instanceof Activity && state instanceof Bundle) {
            ((Bundle) state).remove("android:support:fragments");
            ((Bundle) state).remove("android:fragments");
        }
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstants.PostInitializer;
    }
}
