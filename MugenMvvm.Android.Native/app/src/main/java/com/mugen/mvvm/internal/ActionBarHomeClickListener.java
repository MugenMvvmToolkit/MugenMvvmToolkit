package com.mugen.mvvm.internal;

import android.view.MenuItem;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public final class ActionBarHomeClickListener implements ILifecycleDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        if (lifecycle == LifecycleState.OptionsItemSelected && state instanceof MenuItem && ((MenuItem) state).getItemId() == android.R.id.home) {
            Object actionBar = ActivityMugenExtensions.getActionBar((IActivityView) target);
            if (actionBar != null)
                ViewMugenExtensions.onMemberChanged(actionBar, ViewMugenExtensions.HomeButtonClick, null);
        }
        return true;
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstants.Default;
    }
}
