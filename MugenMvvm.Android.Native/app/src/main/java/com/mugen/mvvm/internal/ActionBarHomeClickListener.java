package com.mugen.mvvm.internal;

import android.view.MenuItem;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.views.ActivityExtensions;
import com.mugen.mvvm.views.ViewExtensions;

public final class ActionBarHomeClickListener implements ILifecycleDispatcher {
    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        if (lifecycle == LifecycleState.OptionsItemSelected && state instanceof MenuItem && ((MenuItem) state).getItemId() == android.R.id.home) {
            Object actionBar = ActivityExtensions.getActionBar((IActivityView) target);
            if (actionBar != null)
                ViewExtensions.onMemberChanged(actionBar, ViewExtensions.HomeButtonClick, null);
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
